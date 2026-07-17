using System;

namespace PH.Core.World
{
    [Serializable]
    public readonly struct FloorAddress
    {
        public FloorAddress(int absoluteFloor, int pageIndex, int pageFloorIndex)
        {
            AbsoluteFloor = absoluteFloor;
            PageIndex = pageIndex;
            PageFloorIndex = pageFloorIndex;
        }

        public int AbsoluteFloor { get; }
        public int PageIndex { get; }
        public int PageFloorIndex { get; }
    }

    [Serializable]
    public sealed class FloorPageData
    {
        public FloorPageData(int pageIndex, int floorsPerPage)
        {
            if (floorsPerPage <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(floorsPerPage), "floorsPerPage는 1 이상이어야 합니다.");
            }

            PageIndex = Math.Max(0, pageIndex);
            FloorsPerPage = floorsPerPage;
            FirstAbsoluteFloor = PageIndex * FloorsPerPage + 1;
            LastAbsoluteFloor = FirstAbsoluteFloor + FloorsPerPage - 1;
        }

        public int PageIndex { get; }
        public int FloorsPerPage { get; }
        public int FirstAbsoluteFloor { get; }
        public int LastAbsoluteFloor { get; }

        public FloorAddress GetAddressByRow(int rowIndex)
        {
            int clampedRowIndex = Math.Max(0, Math.Min(rowIndex, FloorsPerPage - 1));
            return new FloorAddress(FirstAbsoluteFloor + clampedRowIndex, PageIndex, clampedRowIndex);
        }
    }

    public sealed class FloorPageGenerator
    {
        private readonly int floorsPerPage;

        public FloorPageGenerator(int floorsPerPage)
        {
            if (floorsPerPage <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(floorsPerPage), "floorsPerPage는 1 이상이어야 합니다.");
            }

            this.floorsPerPage = floorsPerPage;
        }

        public FloorPageData GeneratePage(int pageIndex)
        {
            return new FloorPageData(Math.Max(0, pageIndex), floorsPerPage);
        }

        public FloorAddress GetAddress(int absoluteFloor)
        {
            int normalizedFloor = Math.Max(1, absoluteFloor);
            int zeroBasedFloor = normalizedFloor - 1;
            int pageIndex = zeroBasedFloor / floorsPerPage;
            int pageFloorIndex = zeroBasedFloor % floorsPerPage;

            return new FloorAddress(normalizedFloor, pageIndex, pageFloorIndex);
        }
    }
}
