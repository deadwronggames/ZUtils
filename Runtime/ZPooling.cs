using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

namespace DeadWrongGames.ZUtils
{
    // TODO maybe make this a service MB instead
    
    /// <summary>
    /// Provides centralized pooling functionality for any Unity component using Object Pools. 
    /// Each pool is associated with a specific prefab to manage its instances.
    /// pools are deleted on scene change
    /// </summary>
    public static class ZPooling<TComponent> where TComponent : Component
    {
        private static readonly Dictionary<GameObject, IObjectPool<TComponent>> s_poolDict = new();

        // clear pools on scene change
        static ZPooling()
        {
            SceneManager.activeSceneChanged += (_, _) => ClearPools(); // I think in general it works as intended but in editor (with quick play options enabled) when starting directly into a map, then it does not get invoked
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] // TODO does it work with regard to above comment? don't think so
        private static void ClearPools()
        {
            "Clearing Pools()".Log(level: ZMethodsDebug.LogLevel.Info);
            foreach (IObjectPool<TComponent> pool in s_poolDict.Values) pool?.Clear();
            s_poolDict.Clear();
        }
        
        /// <summary>
        /// Gets a pooled component, or creates a new pool if none exists for the provided prefab.
        /// A valid prefab must be provided to identify the corresponding component pool.
        /// Pool parameters are set during the first call to this method and are used for all subsequent requests for the same prefab.
        /// </summary>
        /// <param name="prefab">The prefab to instantiate and pool.</param>
        /// <param name="createFunc">Custom creation logic for the pooled component (optional, defaults to instantiating the prefab).</param>
        /// <param name="actionOnGet">Action to perform when the component is retrieved from the pool (optional, defaults to activating the GameObject).</param>
        /// <param name="actionOnRelease">Action to perform when the component is released back into the pool (optional, defaults to deactivating the GameObject).</param>
        /// <param name="actionOnDestroy">Action to perform when the component is destroyed (optional, defaults to destroying the GameObject).</param>
        /// <param name="maxSize">The maximum size of the pool.</param>
        /// <returns>Returns a wrapped component with a method for releasing it back to the pool.</returns>
        public static ZPooledComponent<TComponent> GetPooledComponent(
            GameObject prefab, 
            Func<TComponent> createFunc = null, 
            Action<TComponent> actionOnGet = null, 
            Action<TComponent> actionOnRelease = null, 
            Action<TComponent> actionOnDestroy = null, 
            int maxSize = int.MaxValue
        )
        {
            if (prefab == null)
            {
                "Prefab is null. Returning default.".Log(level: ZMethodsDebug.LogLevel.Warning);
                return default;
            }

            if (!s_poolDict.TryGetValue(prefab, out IObjectPool<TComponent> pool))
            {
                // validate prefab
                if (prefab.GetComponent<TComponent>() == null) 
                {
                   $"Prefab does not have a Component {nameof(TComponent)}. Returning default.".Log(level: ZMethodsDebug.LogLevel.Warning);
                    return default;
                }    
                    
                // create new pool
                s_poolDict[prefab] = (pool = new ObjectPool<TComponent>(
                    createFunc: createFunc           ?? (() => UnityEngine.Object.Instantiate(prefab).GetComponent<TComponent>()), // per default, just instantiate prefab
                    actionOnGet: actionOnGet         ?? (component => component.gameObject.SetActive(true)), // per default, just activate GO
                    actionOnRelease: actionOnRelease ?? (component => component.gameObject.SetActive(false)), // per default, just deactivate GO
                    actionOnDestroy: actionOnDestroy ?? (component => { if (component != null) UnityEngine.Object.Destroy(component.gameObject); }), // per default, just destroy GO
                    maxSize: maxSize
                ));
            }
            
            TComponent component = pool.Get();
            return new ZPooledComponent<TComponent>(component, pool);
        }
    }
    
    /// <summary>
    /// Wrapper for a pooled component that allows it to be released back to its pool.
    /// </summary>
    /// <typeparam name="TPooledObject">The type of component being pooled.</typeparam>
    public struct ZPooledComponent<TPooledObject> where TPooledObject : Component
    {
        public TPooledObject Component { get; }
        private readonly IObjectPool<TPooledObject> _ownerPool;

        public ZPooledComponent(TPooledObject component, IObjectPool<TPooledObject> ownerPool)
        {
            Component = component;
            _ownerPool = ownerPool;
        }

        public void Release()
        {
            if (Component != null && _ownerPool != null) _ownerPool.Release(Component);
        }
    }
}