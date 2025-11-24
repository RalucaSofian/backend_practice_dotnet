using PetRescue.Utilities;


namespace PetRescue.ApiDTOs;

public class PaginatedListDTO<T>
{
    public int PageNumber { get; private set; }
    public int TotalPages { get; private set; }
    public int ItemCount { get; private set; }
    public List<T>? Items { get; private set; }

    public PaginatedListDTO() { }

    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public static PaginatedListDTO<T> ConvertList<S>(List<T> newItems, PaginatedList<S> existingList)
    {
        var result = new PaginatedListDTO<T>()
        {
            PageNumber = existingList.PageNumber,
            TotalPages = existingList.TotalPages,
            ItemCount = existingList.ItemCount,
            Items = newItems
        };

        return result;
    }
}
