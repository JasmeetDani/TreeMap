using System.Data.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TreeMapControllerCustomDraw : MonoBehaviour
{
    [SerializeField]
    private TreeMapViewCustomDraw view;

    private TreeMapModel model = null;


    [SerializeField]
    private Text Info;

    [SerializeField]
    private Text LeafInfo;


    public void Init<T>(DataContext sourceData, string[] columnsToSortBy, string defaultColumnForSize, string defaultColumnForColor) where T : class
    {
        model = new TreeMapModel();

        model.LoadData<T>(sourceData, columnsToSortBy, defaultColumnForSize, defaultColumnForColor);

        view.ResetZoom();

        view.DrawHeirarchy(model.Root);
    }


    public Color LookUp(float weight)
    {
        return model.getColor(weight);
    }

    public TreeMapNode LookUp(int ID)
    {
        return model.IDLookup[ID];
    }


    public void OnNeedToShowRecordInfo(string ID)
    {
        LeafInfo.text = model.GetData<Data>(ID);
    }

    public void OnNeedToShowFrameInfo(int nodeID)
    {
        TreeMapNode node = model.IDLookup[nodeID];


        string message = string.Format("\nTotal Weight : {0}\n" +
                             "Total Count : {1}", node.Data.SizeByWeight, node.Data.TotalCount);

        while (node.Parent != null)
        {
            message = node.Data.TreeNodeDataType.Name + " : " + node.Data.Value.ToString() + "\n" + message;

            node = node.Parent;
        }

        Info.text = message;
    }
}