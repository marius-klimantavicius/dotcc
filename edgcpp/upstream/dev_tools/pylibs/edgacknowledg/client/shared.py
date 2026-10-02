# Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
# Exceptions.
# See https://edgcpp.org/LICENSE.txt for license information.
# SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception

'''Shared AcknowlEDG client types used by test and bench providers.'''

import asyncio
import os

from typing import (
  Any,
  Awaitable,
  Callable,
  Dict,
  List,
  Optional,
  Union,
)

try:
  from typing import Protocol
except ImportError:
  try:
    from typing_extensions import Protocol  # type: ignore
  except ImportError:
    Protocol = None  # type: ignore[misc,assignment]

from edgacknowledg import (
  MAX_DISPLAY_CONTENT_SIZE,
  ProtocolMessage,
  RequestTags,
  message,
)


if Protocol is not None:
  class Channel(Protocol):
    async def recv(self) -> Any: ...
    async def send(self, msg: ProtocolMessage) -> None: ...


  class ReviewIdentity(Protocol):
    @property
    def name(self) -> str: ...
    @property
    def timestamp(self) -> str: ...


  class ReviewClientHandler(Protocol):
    '''Structural provider for one review section subscription.'''

    @property
    def subscription_type(self) -> str:
      '''One of: test.src, test.edit, bench.src, bench.edit'''
      ...

    @property
    def section_name(self) -> str:
      ...

    @property
    def slug(self) -> str:
      ...

    async def handle_request(self, channel: Channel) -> None:
      '''Process one inbound request on ``channel``.'''
      ...
else:
  Channel = Any  # type: ignore[misc,assignment]
  ReviewIdentity = Any  # type: ignore[misc,assignment]
  ReviewClientHandler = Any  # type: ignore[misc,assignment]


from edgutil import get_running_event_loop

CoroFactory = Callable[[], Awaitable[Any]]


class BackgroundWorkerPool:
  '''Async worker pool used to regulate parallel background work.'''

  def __init__(self, max_workers: Optional[int] = None) -> None:
    cpu_count = os.cpu_count()
    self._max_workers = max_workers if max_workers is not None else (
      cpu_count if cpu_count is not None else 1
    )
    # Queue element type: Tuple[CoroFactory, asyncio.Future]
    self._queue = asyncio.Queue(self._max_workers)  # type: asyncio.Queue
    self._workers = []  # type: List[asyncio.Task]
    self._started = False

  def ensure_started(self) -> None:
    if self._started:
      return
    self._started = True
    loop = get_running_event_loop()
    for _ in range(self._max_workers):
      self._workers.append(loop.create_task(self._worker()))

  async def _worker(self) -> None:
    while True:
      coro_factory, future = await self._queue.get()
      try:
        result = await coro_factory()
        if not future.done():
          future.set_result(result)
      except Exception as ex:
        if not future.done():
          future.set_exception(ex)
      finally:
        self._queue.task_done()

  async def submit(self, coro_factory: CoroFactory) -> Any:
    '''Run ``coro_factory()`` on a pool worker and return its result.'''
    self.ensure_started()
    loop = get_running_event_loop()
    future = loop.create_future()  # type: asyncio.Future
    await self._queue.put((coro_factory, future))
    return await future


async def announce_section(channel: Channel,
                           handler: ReviewClientHandler,
                           review: ReviewIdentity) -> None:
  '''Send optional section/review identity messages on a new subscription.'''
  await channel.send(message(
    RequestTags.META_SECTION_NAME,
    value = handler.section_name
  ))
  await channel.send(message(
    RequestTags.META_REVIEW_NAME,
    value = review.name
  ))
  await channel.send(message(
    RequestTags.META_REVIEW_TIMESTAMP,
    value = review.timestamp
  ))


def trunc_display_content(content: str) -> Dict[str, Union[str, bool]]:
  '''Truncate ``content`` for UI display; may set ``truncated``.'''
  max_content_size = MAX_DISPLAY_CONTENT_SIZE
  if len(content) > max_content_size:
    # Attempt to truncate at line boundaries.
    #
    # To save time, find the newline closest to the boundary before walking
    # the file size back.
    last_newline_permitted = content.find('\n', max_content_size)
    if last_newline_permitted != -1:
      content = content[:last_newline_permitted]
    # Now walk the file size back.
    while len(content) > max_content_size:
      last_new_line_idx = content.rfind('\n')
      if last_new_line_idx != -1:
        content = content[:(last_new_line_idx - 1)]
      else:
        # If there are no more newlines, resort to truncating within
        # the line.
        content = content[:(max_content_size - 4)]
        content += '...'
    return {
      'truncated': True,
      'content': content
    }
  return { 'content': content }
