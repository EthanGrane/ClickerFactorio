using UnityEngine;

public static class MathUtils
{
    public static int Hash(int x, int y, int seed)
    {
        unchecked
        {
            int h = seed;
            h = h * 73856093 ^ x;
            h = h * 19349663 ^ y;
            return h;
        }
    }

    public static float Random01(int x, int y, int seed)
    {
        unchecked
        {
            long h = seed;
            h = (h ^ (x * 374761393)) * 668265263;
            h = (h ^ (y * 1274126177)) * 2246822519;
            h ^= (h >> 13);
            h *= 3266489917;
            h ^= (h >> 16);
            return (h & 0x7fffffff) / (float)int.MaxValue;
        }
    }


}
