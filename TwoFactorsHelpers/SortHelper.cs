
using System.Linq;
namespace LapoLoanWebApi.TwoFactorsHelpers
{
    public class SortHelper
    {
        //public IQueryable<T> FilterBy(IQueryable<T> myProcess, string filterBy, string filterValue)
        //{
        //    var listFilter = filterBy.Split(',');
        //    var listValues = filterValue.Split(',');

        //    for (int index = 0; index < listFilter.Length; index++)
        //    {
        //        var filter = listFilter[index];
        //        var value = listValues[index];

        //        switch (filter)
        //        {
        //            case "type":
        //                myProcess = myProcess.Where(c => c.Status.Nome.Contains(value));
        //                break;
        //            case "created":
        //                myProcess = myProcess.Where(c => c.Created - DateTime.Parse(value) >= new TimeSpan(0, 0, 0));
        //            default:
        //                break;
        //        }
        //    }

        //    return myProcess;
        //}


        //public  IQueryable<T> OrderBy(IQueryable<T> myProcess, string attribute, string direction)
        //{
        //    switch (attribute)
        //    {
        //        case "type":
        //            myProcess = (direction == "asc")
        //                ? myProcess.OrderBy(c => c.Type)
        //                : myProcess.OrderByDescending(c => c.Type);
        //            return myProcess;
        //        case "created":
        //            myProcess = (direction == "asc")
        //                ? myProcess.OrderBy(c => c.Created)
        //                : myProcess.OrderByDescending(c => c.Created);
        //            return myProcess;
        //    }

        //    return myProcess;
        //}
    }
}
