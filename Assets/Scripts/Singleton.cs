// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;


/// <summary>
/// Singleton behaviour class, used for components that should only have one instance.
/// <remarks>Singleton classes live on through scene transitions and will mark their 
/// parent root GameObject with <see cref="Object.DontDestroyOnLoad"/></remarks>
/// </summary>
/// <typeparam name="T">The Singleton Type</typeparam>
public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T instance;

    /// <summary>
    /// Returns the Singleton instance of the classes type.
    /// If no instance is found, then we search for an instance
    /// in the scene.
    /// If more than one instance is found, we throw an error and
    /// no instance is returned.
    /// </summary>
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                T[] objects = FindObjectsOfType<T>();
                if (objects.Length == 1)
                    instance = objects[0];
                else if (objects.Length > 1)
                    Debug.LogErrorFormat("Expected exactly 1 {0} but found {1}.", typeof(T).Name, objects.Length);

                if (instance != null)
                    instance.EnsureInitialized();
            }

            return instance;
        }
    }


    private bool initialized = false;

    private void EnsureInitialized()
    {
        if (this.initialized) return;
        this.initialized = true;
        this.OnInit();
    }

    /// <summary>
    /// Base Awake method that sets the Singleton's unique instance.
    /// Called by Unity when initializing a MonoBehaviour.
    /// Scripts that extend Singleton should be sure to call base.Awake() to ensure the
    /// static Instance reference is properly created.
    /// </summary>
    protected virtual void Awake()
    {
        Debug.Log("Awake the Singleton class");
        if (instance != null && instance != this)
        {
            if (Application.isEditor)
                DestroyImmediate(this);
            else
                Destroy(this);

            Debug.LogErrorFormat("Trying to instantiate a second instance of singleton class {0}. " +
                                 "Additional Instance was destroyed", GetType().Name);
        }
        else if (instance == null)
        {
            instance = (T)this;
            this.EnsureInitialized();
        }
    }

    /// <summary>
    /// Base OnDestroy method that destroys the Singleton's unique instance.
    /// Called by Unity when destroying a MonoBehaviour. Scripts that extend
    /// Singleton should be sure to call base.OnDestroy() to ensure the
    /// underlying static Instance reference is properly cleaned up.
    /// </summary>
    protected virtual void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }

        this.initialized = false;
    }

    /// <summary>
    /// Called when the <see cref="Singleton{T}"/> is initialized. Depending on the
    /// applications call structure, this may happen before or after <see cref="Awake"/>,
    /// but it is guaranteed to run before the instance is retrieved for the first time.
    /// 
    /// As a result, some singletons are better off using <see cref="OnInit"/> than
    /// <see cref="Awake"/> for their init code.
    /// </summary>
    protected virtual void OnInit()
    {
    }
}