using Molca;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : Component
{
    private List<T> activeObjects;
    private List<T> pooledObjects;
    private T _prefab;
    private Transform _prefabRoot;

    private int _totalObjects;

    public System.Action<T> onObjectReturned;

    public ObjectPool(T prefab, int poolSize, Transform root)
    {
        activeObjects = new List<T>();
        pooledObjects = new List<T>();
        _prefab = prefab;
        _prefabRoot = root;
        RuntimeManager.RunCoroutine(IncreaseSizeAsync(poolSize));
    }

    public IEnumerator IncreaseSizeAsync(int value)
    {
        for (int i = 0; i < value; i++)
        {
            var async = Object.InstantiateAsync(_prefab, _prefabRoot);
            while (!async.isDone) yield return new WaitForEndOfFrame();
            T newObject = async.Result[0];
            newObject.gameObject.SetActive(false);
            pooledObjects.Add(newObject);
        }

        _totalObjects += value;
    }

    private void IncreaseSize(int value)
    {
        for (int i = 0; i < value; i++)
        {
            T newObject = Object.Instantiate(_prefab, _prefabRoot);
            newObject.gameObject.SetActive(false);
            pooledObjects.Add(newObject);
        }

        _totalObjects += value;
    }

    public T GetObject()
    {
        T objectToReturn = null;
        if (pooledObjects.Count == 0)
            IncreaseSize(1);

        if (pooledObjects.Count > 0)
        {
            objectToReturn = pooledObjects[0];
            pooledObjects.RemoveAt(0);
            objectToReturn.gameObject.SetActive(true);
        }

        activeObjects.Add(objectToReturn);
        return objectToReturn;
    }

    public void ReturnObject(T objectToReturn)
    {
        objectToReturn.gameObject.SetActive(false);
        pooledObjects.Add(objectToReturn);
        activeObjects.Remove(objectToReturn);

        onObjectReturned?.Invoke(objectToReturn);
    }

    public void ReturnAll()
    {
        while(activeObjects.Count > 0)
            ReturnObject(activeObjects[0]);
    }
}