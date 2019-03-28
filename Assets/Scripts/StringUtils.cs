
public static class StringUtils
{
    public static int Difference(string str1, string str2)
    {
        if (str1.Equals(str2))
        {
            return 0;
        }

        string[] arr1 = str1.Split(new char[] { '|' });
        string[] arr2 = str2.Split(new char[] { '|' });

        int ret = arr1.Length;

        for (int i = 0; i < arr1.Length; ++i)
        {
            if (arr1[i].Equals(arr2[i]))
            {
                --ret;

                continue;
            }

            return ret;
        }

        // We shouldn't get here

        return 0;
    }
}