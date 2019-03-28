using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using UnityEngine;

public class TreeMapModel
{
    private TreeMapNode root = new TreeMapNode();
    
    public TreeMapNode Root
    {
        get { return root; }

        private set { }
    }


    // For bookkeeping

    DataContext source;

    TreeMapNode[] nodes;

    int ID = 0;

    List<string> Paths = new List<string>();

    string pkfield;


    // Tree node lookup

    private Dictionary<int, TreeMapNode> idLookup = new Dictionary<int, TreeMapNode>();

    public Dictionary<int, TreeMapNode> IDLookup
    {
        get { return idLookup; }

        private set { }
    }


    // Color lookup

    private ColorIntervals colorIntervals = new ColorIntervals();


    public Color getColor(float weight)
    {
        return colorIntervals.Lookup(weight);
    }


    public void LoadData<T>(DataContext sourceData, string[] columnsToSortBy, string defaultColumnForSize, string defaultColumnForColor) where T : class
    {
        this.source = sourceData;


        var dataMembers = sourceData.Mapping.GetTable(typeof(T)).RowType.PersistentDataMembers;

        IEnumerable<DataType> cols = from col in dataMembers
                                     join colname in columnsToSortBy on col.MappedName equals colname
                                     select new DataType()
                                     {
                                         MappedName = col.MappedName,

                                         Name = col.Name,

                                         RealType = col.Type
                                     };

        List<DataType> listCols = cols.ToList();

        listCols.Add(new DataType
                        {
                            MappedName = defaultColumnForSize,

                            Name = dataMembers.Single((c) => c.MappedName.Equals(defaultColumnForSize)).Name,

                            RealType = typeof(float)
                        });


        // Create the TreeMap

        string columnForColor = dataMembers.Single((c) => c.MappedName.Equals(defaultColumnForColor)).Name;

        ConstructTreeMap<T>(sourceData, listCols.ToArray(), columnForColor);


        // Initialize color lookup

        ColorIntervals temp = new ColorIntervals();

        temp.AddInterval(new FloatRange { Start = 0.01f, End = 5.68f },
            236, 236, 240, 181, 181, 254);
        temp.AddInterval(new FloatRange { Start = 5.68f, End = 8.88f },
            169, 184, 255, 5, 252, 255);
        temp.AddInterval(new FloatRange { Start = 8.88f, End = 12.08f },
            0, 254, 242, 0, 242, 47);
        temp.AddInterval(new FloatRange { Start = 12.08f, End = 17.20f },
            15, 242, 40, 247, 254, 1);
        temp.AddInterval(new FloatRange { Start = 17.20f, End = 51.33f },
            255, 241, 1, 255, 38, 31);
        temp.AddInterval(new FloatRange { Start = 51.33f, End = float.MaxValue },
            255, 32, 32, 255, 32, 32);

        colorIntervals = temp;
    }

    public string GetData<T>(string ID) where T : class
    {
        var table = source.GetTable<T>();

        var param = Expression.Parameter(typeof(T), "e");

        var predicate = Expression.Lambda<Func<T, bool>>(Expression.Equal(Expression.Field(param, pkfield), Expression.Constant(ID)), param);

        return table.Where(predicate).ToArray()[0].ToString();
    }

    
    private void ConstructTreeMap<T>(DataContext sourceData, DataType[] columns, string defaultColumnForColor) where T : class
    {
        // Compute the hashes for the fields
        string temp = string.Empty;

        bool bRejectRow = false;


        StringBuilder query = new StringBuilder();

        query.Append("Select * from {0} order by ");

        for (int i = 0; i < (columns.Length - 2);++i)
        {
            query.Append("\"");
            query.Append(columns[i].MappedName);
            query.Append("\"");
            query.Append(",");
        }

        query.Append(columns[columns.Length - 2].MappedName);

        List<T> table = sourceData.ExecuteQuery<T>(string.Format(query.ToString(), typeof(T).Name)).ToList<T>();
                
        foreach (T row in table)
        {
            for (int i = 0; i < columns.Length - 1; ++i)
            {
                string tmp = typeof(T).GetField(columns[i].Name, BindingFlags.Public | BindingFlags.Instance).GetValue(row).ToString();

                if (tmp == string.Empty)
                {
                    bRejectRow = true;

                    temp = string.Empty;

                    break;
                }

                temp = temp + "|" + tmp;
            }

            Paths.Add(temp);

            temp = string.Empty;


            if (bRejectRow)
                bRejectRow = false;
        }

        
        // Initialize the root

        root.Data = new TreeMapNodeData();

        root.ID = 0;

        idLookup.Add(root.ID, root);


        // Create the bookkeeping nodes

        nodes = new TreeMapNode[columns.Length + 1];

        nodes[0] = root;


        // Get the primary key column

        var mapping = sourceData.Mapping.GetTable(typeof(T));

        pkfield = mapping.RowType.PersistentDataMembers.Single(d => d.IsPrimaryKey).Name;


        // Create heirarchy using first record

        bool init = false;

        if (Paths[0] != string.Empty)
        {
            if (CreateHeirarchy(1, 0, table, columns, defaultColumnForColor))
            {
                init = true;
            }
        }


        // Save the last inserted node

        int level = init ? columns.Length : 1;


        // Create the remaining nodes

        for (int i = 1, j = init ? i - 1 : 1; i < table.Count; ++i, j = init ? j : j + 1)
        {
            if (Paths[i] != string.Empty)
            {
                int diff = StringUtils.Difference(Paths[j], Paths[i]);

                if (diff == 0)
                {
                    if (CreateHeirarchy(level, i, table, columns, defaultColumnForColor))
                    {
                        if (!init)
                        {
                            level = columns.Length;

                            init = true;
                        }

                        j = i;
                    }
                }
                else
                {
                    if (CreateHeirarchy(level - diff, i, table, columns, defaultColumnForColor))
                    {
                        j = i;
                    }
                }
            }
        }
    }

    
    private bool CreateHeirarchy<T>(int startLevel, int recNo, List<T> data, DataType[] columns,
                                 string defaultColumnForColor) where T : class
    {
        int i;


        TreeMapNode current = nodes[startLevel - 1];


        string cellForSize = typeof(T).GetField(columns[columns.Length - 1].Name,
                                       BindingFlags.Public | BindingFlags.Instance).GetValue(data[recNo]).ToString();

        string cellForColor = typeof(T).GetField(defaultColumnForColor,
                               BindingFlags.Public | BindingFlags.Instance).GetValue(data[recNo]).ToString();

        float weightSize = 0, weightColor = 0;

        if (!String.IsNullOrEmpty(cellForSize) && !String.IsNullOrEmpty(cellForColor))
        {
            weightSize = Convert.ToSingle(cellForSize);

            weightColor = Convert.ToSingle(cellForColor);
        }

        if (weightSize <= 0)
        {
            return false;
        }


        for (i = 1; i <= (startLevel - 1); ++i)
        {
            nodes[i - 1].Data.SizeByWeight += Math.Abs(weightSize);

            nodes[i - 1].Data.TotalCount += 1;
        }

        for (i = startLevel; i <= columns.Length; ++i)
        {
            TreeMapNode newNode = new TreeMapNode();


            nodes[i] = newNode;


            newNode.Data = new TreeMapNodeData();

            newNode.ID = ++ID;

            idLookup.Add(newNode.ID, newNode);

            newNode.Data.TreeNodeDataType = columns[i - 1];

            newNode.Data.Value = typeof(T).GetField(columns[i - 1].Name,
                                       BindingFlags.Public | BindingFlags.Instance).GetValue(data[recNo]);


            current.AddChild(newNode);

            current.Data.SizeByWeight += Math.Abs(weightSize);

            current.Data.TotalCount += 1;


            newNode.Parent = current;

            current = newNode;
        }


        current.Data.SizeByWeight = Math.Abs(weightSize);

        current.Data.ColorByWeight = Math.Abs(weightColor);

        current.Data.rowID = typeof(T).GetField(pkfield,
                                       BindingFlags.Public | BindingFlags.Instance).GetValue(data[recNo]).ToString();


        return true;
    }
}