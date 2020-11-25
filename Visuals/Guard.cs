using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Guard<T>
{
    Dictionary<T, int> values;

    public Guard()
    {
        values = new Dictionary<T, int>();
    }

    // returns true if creates a new entry
    public bool Add(T value)
    {
        if (values.ContainsKey(value))
        {
            values[value]++;
            return false;
        }
        else
        {
            values.Add(value, 0);
            return true;
        }
    }

    // returns true if removes an entire entry
    public bool Remove(T value)
    {
        if (values.ContainsKey(value))
        {
            if (values[value] > 0)
            {
                values[value]--;
            }
            else
            {
                values.Remove(value);
                return true;
            }
        }
        return false;
    }

    public IEnumerable<T> GetValues()
    {
        return values.Keys;
    }
}
