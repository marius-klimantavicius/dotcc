using Cui.Managed;

namespace Cui.ManagedGallery;

public sealed partial class GalleryDemo
{
    public int SelectedGridRows
    {
        get
        {
            CheckAlive();
            var count = 0;
            foreach (var value in aPeopleSelected)
                if (value)
                    count++;
            return count;
        }
    }

    public TableSort GridSort
    {
        get
        {
            CheckAlive();
            return aPeopleSort.Value;
        }
    }

    public bool IsGridRowSelected(int position)
    {
        CheckAlive();
        if ((uint)position >= aPeopleCount) throw new ArgumentOutOfRangeException(nameof(position));

        return aPeopleSelected[position];
    }

    public string GridRowName(int position)
    {
        CheckAlive();
        if ((uint)position >= aPeopleCount) throw new ArgumentOutOfRangeException(nameof(position));
        if (aPeople is null) throw new InvalidOperationException("Build the Data grid page before reading its rows.");

        return aPeople[aPeopleOrder[position]].Name;
    }
}