using UnityEngine;
using System.Data.Linq;
using Mono.Data.Sqlite;
using System.Linq;

public class TreeMapTest : MonoBehaviour
{
    [SerializeField]
    private TreeMapControllerCustomDraw treeMapController;


    [SerializeField]
    private string[] columnsToSortBy;


    void Start()
    {
        var connection = new SqliteConnection(string.Format(@"Data Source={0};Version=3;Read Only=True",
            Application.streamingAssetsPath + "\\" + "TreeMapTest.db"));

        var context = new DataContext(connection);

        if (context.DatabaseExists())
        {
            var dataMembers = context.Mapping.GetTable(typeof(Data)).RowType.PersistentDataMembers;

            if (columnsToSortBy.Length < dataMembers.Count)
            {
                var cols = (from col in dataMembers
                            join colname in columnsToSortBy on col.MappedName equals colname
                            select col.MappedName).ToList();

                if (cols.Count == columnsToSortBy.Length)
                {
                    treeMapController.Init<Data>(context, columnsToSortBy, "Occupancy", "Occupancy");
                }
                else
                {
                    var colsnotfound = columnsToSortBy.Except(cols).ToList();

                    colsnotfound.ForEach((c) => { Debug.LogError(string.Format("{0} not found in database", c)); });
                }
            }
            else
            {
                Debug.LogError(string.Format("Max number of columns to sort by is {0}", dataMembers.Count - 1));
            }
        }
        else
        {
            Debug.LogError("Error connecting to DB");
        }
    }
}