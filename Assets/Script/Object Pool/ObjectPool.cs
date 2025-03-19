using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : BaseObject
{
    [SerializeField] private int preparedObjectCount = 10;
    [SerializeField] private GameObject prefab;

    private List<PoolingObject> restingList = new List<PoolingObject>(10);
    private List<PoolingObject> usingList = new List<PoolingObject>(10);

    private WaitForEndOfFrame wait = new WaitForEndOfFrame();
    private Coroutine objectCreater;

    private int count = 1;

    protected override void Start()
    {
        base.Start();

        PrepareInstance();
    }

    public T Burrow<T>() where T : PoolingObject
    {
        PoolingObject burrowObject;

        if (restingList.Count > 0)
        {
            burrowObject = restingList[0];

            restingList.RemoveAt(0);
            usingList.Add(burrowObject);
        }
        else
        {
            GameObject go = GameObject.Instantiate(prefab, this.transform);
            go.name = $"{go.name}_{count++}";

            burrowObject = go.GetComponent<PoolingObject>();
            burrowObject.SetPool(this);
            usingList.Add(burrowObject);
        }

        if (burrowObject is T)
        {
            return (T)burrowObject;
        }
        else
        {
            throw new System.ArgumentException();
        }
    }

    public int GetRestCount()
    {
        return restingList.Count;
    }

    public int GetBurrowedCount()
    {
        return usingList.Count;
    }

    public void Return(PoolingObject poolingObject)
    {
        if (usingList.Contains(poolingObject) == false)
        {
            Debug.LogError("잘못된 반환");

            return;
        }

        usingList.Remove(poolingObject);
        restingList.Add(poolingObject);
    }

    private void PrepareInstance()
    {
        if (objectCreater == null)
        {
            objectCreater = StartCoroutine(ObjectCreate());
        }
    }

    private IEnumerator ObjectCreate()
    {
        int breadCount = preparedObjectCount;

        if (prefab.GetComponent<PoolingObject>() != null)
        {
            while (breadCount-- > 0)
            {
                GameObject go = GameObject.Instantiate(prefab, this.transform);
                go.name = $"{go.name}_{count++}";

                PoolingObject newObject = go.GetComponent<PoolingObject>();
                newObject.SetPool(this);
                newObject.Disable();
                restingList.Add(newObject);

                yield return wait;
            }
        }
        else
        {
            Debug.LogError("프리팹 오류 발견");
        }

        objectCreater = null;
    }
}
