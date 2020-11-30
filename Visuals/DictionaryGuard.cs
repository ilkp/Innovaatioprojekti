using System.Collections.Generic;

public class DictionaryGuard<TKey,TValue>
{
    private readonly Dictionary<TKey, TValue> values;
    private readonly Dictionary<TKey, int> counts;

    public DictionaryGuard()
    {
        values = new Dictionary<TKey, TValue>();
        counts = new Dictionary<TKey, int>();
    }

    // returns true if creates a new entry
    public bool Add(TKey key, TValue value)
    {
        if (values.ContainsKey(key))
        {
            counts[key]++;
            return false;
        }
        else
        {
            values.Add(key, value);
            counts.Add(key, 0);
            return true;
        }
    }

    // returns true if removes an entire entry
    public bool Remove(TKey key)
    {
        if (values.ContainsKey(key))
        {
            if (counts[key] > 0)
            {
                counts[key]--;
            }
            else
            {
                values.Remove(key);
                counts.Remove(key);
                return true;
            }
        }
        return false;
    }

    public IEnumerable<TValue> GetValues()
    {
        return values.Values;
    }

    public int GetCount()
    {
        return values.Count;
    }
}
