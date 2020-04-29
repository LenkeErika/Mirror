using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirror.Tests
{
    [TestFixture]
    public class ClientSceneTests
    {
        // use guid to find asset so that the path does not matter
        const string ValidPrefabAssetGuid = "33169286da0313d45ab5bfccc6cf3775";
        const string InvalidPrefabAssetGuid = "78f0a3f755d35324e959f3ecdd993fb0";

        GameObject validPrefab;
        GameObject invalidPrefab;
        Guid validPrefabGuid;
        Guid invalidPrefabGuid;

        Dictionary<Guid, GameObject> prefabs => ClientScene.prefabs;
        Dictionary<Guid, SpawnHandlerDelegate> spawnHandlers => ClientScene.spawnHandlers;
        Dictionary<Guid, UnSpawnDelegate> unspawnHandlers => ClientScene.unspawnHandlers;

        static GameObject LoadPrefab(string guid)
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            validPrefab = LoadPrefab(ValidPrefabAssetGuid);
            invalidPrefab = LoadPrefab(InvalidPrefabAssetGuid);
            validPrefabGuid = new Guid(ValidPrefabAssetGuid);
            invalidPrefabGuid = new Guid(ValidPrefabAssetGuid);
        }

        [TearDown]
        public void TearDown()
        {
            ClientScene.Shutdown();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            validPrefab = null;
            invalidPrefab = null;
        }

        [Test]
        [Ignore("Not Implemented")]
        public void PrepareToSpawnSceneObjects()
        {
            // Finds All inactive Network Identities In Scene With SceneIds
            // ClearsExistingDictionary
            ClientScene.PrepareToSpawnSceneObjects();


            // Need to create objects in scene
            // make sure they are disabled and have sceneId
            // then check if they are added to dictionary 
            int someNumber = 2;
            Assert.That(ClientScene.spawnableObjects, Has.Count.EqualTo(someNumber));
        }


        [Test]
        public void GetPrefab_ReturnsFalseForEmptyGuid()
        {
            bool result = ClientScene.GetPrefab(new Guid(), out GameObject prefab);

            Assert.IsFalse(result);
            Assert.IsNull(prefab);
        }

        [Test]
        public void GetPrefab_ReturnsFalseForPrefabNotFound()
        {
            Guid guid = Guid.NewGuid();
            bool result = ClientScene.GetPrefab(guid, out GameObject prefab);

            Assert.IsFalse(result);
            Assert.IsNull(prefab);
        }

        [Test]
        public void GetPrefab_ReturnsFalseForPrefabIsNull()
        {
            Guid guid = Guid.NewGuid();
            prefabs.Add(guid, null);
            bool result = ClientScene.GetPrefab(guid, out GameObject prefab);

            Assert.IsFalse(result);
            Assert.IsNull(prefab);
        }

        [Test]
        public void GetPrefab_ReturnsTrueWhenPrefabIsFound()
        {
            prefabs.Add(validPrefabGuid, validPrefab);
            bool result = ClientScene.GetPrefab(validPrefabGuid, out GameObject prefab);

            Assert.IsTrue(result);
            Assert.NotNull(prefab);
        }

        [Test]
        public void GetPrefab_HasOutPrefabWithCorrectGuid()
        {
            prefabs.Add(validPrefabGuid, validPrefab);
            ClientScene.GetPrefab(validPrefabGuid, out GameObject prefab);


            Assert.NotNull(prefab);

            NetworkIdentity networkID = prefab.GetComponent<NetworkIdentity>();
            Assert.AreEqual(networkID.assetId, validPrefabGuid);
        }


        [Test]
        [Ignore("Not Implemented")]
        public void RegisterPrefab()
        {
            // overloads
            // RegisterPrefab(GameObject prefab)
            // RegisterPrefab(GameObject prefab, SpawnDelegate spawnHandler, UnSpawnDelegate unspawnHandler)
            // RegisterPrefab(GameObject prefab, SpawnHandlerDelegate spawnHandler, UnSpawnDelegate unspawnHandler)
            // RegisterPrefab(GameObject prefab, Guid newAssetId)


            // Adds Prefab To Prefab Dictionary
            // gives errors when...
            //   prefab is null
            //   prefab does not have NetworkIdentity
            // gives warning when...
            //   prefab has multiple prefab does not have NetworkIdentity
            //   assetId is already being used by different prefab/handler
            ClientScene.RegisterPrefab(null);

            // Sets newAssetId
            // gives warning when...
            //   assetId is already being used by different prefab/handler
            ClientScene.RegisterPrefab(null, new Guid());

            // Adds Delegate to spawnHandlers dictionary
            // Adds Delegate to unspawnHandlers dictionary
            // gives errors when...
            //   prefab is null
            //   prefab does not have NetworkIdentity
            //   spawnHandler is null
            //   unspawnHandler is null
            //   assetId is empty
            // gives warning when...
            //   assetId is already being used by different prefab/handler
            ClientScene.RegisterPrefab(null, new SpawnDelegate((x, y) => null), new UnSpawnDelegate((x) => { }));
            ClientScene.RegisterPrefab(null, new SpawnHandlerDelegate((x) => null), new UnSpawnDelegate((x) => { }));
        }


        [Test]
        public void UnregisterPrefab_RemovesPrefabFromDictionary()
        {
            prefabs.Add(validPrefabGuid, validPrefab);

            ClientScene.UnregisterPrefab(validPrefab);

            Assert.IsFalse(prefabs.ContainsKey(validPrefabGuid));
        }

        [Test]
        public void UnregisterPrefab_RemovesSpawnHandlerFromDictionary()
        {
            spawnHandlers.Add(validPrefabGuid, new SpawnHandlerDelegate(x => null));

            ClientScene.UnregisterPrefab(validPrefab);

            Assert.IsFalse(spawnHandlers.ContainsKey(validPrefabGuid));
        }

        [Test]
        public void UnregisterPrefab_RemovesUnSpawnHandlerFromDictionary()
        {
            unspawnHandlers.Add(validPrefabGuid, new UnSpawnDelegate(x => { }));

            ClientScene.UnregisterPrefab(validPrefab);

            Assert.IsFalse(unspawnHandlers.ContainsKey(validPrefabGuid));
        }

        [Test]
        public void UnregisterPrefab_ErrorWhenPrefabIsNull()
        {
            LogAssert.Expect(LogType.Error, "Could not unregister prefab because it was null");
            ClientScene.UnregisterPrefab(null);
        }

        [Test]
        public void UnregisterPrefab_ErrorWhenPrefabHasNoNetworkIdentity()
        {
            LogAssert.Expect(LogType.Error, $"Could not unregister '{invalidPrefab.name}' since it contains no NetworkIdentity component");
            ClientScene.UnregisterPrefab(invalidPrefab);
        }


        [Test]
        public void RegisterSpawnHandler_SpawnDelegate_AddsHandlerToSpawnHandlers()
        {
            int handlerCalled = 0;

            Guid guid = Guid.NewGuid();
            SpawnDelegate spawnHandler = new SpawnDelegate((pos, rot) =>
            {
                handlerCalled++;
                return null;
            });
            UnSpawnDelegate unspawnHandler = new UnSpawnDelegate(x => { });

            ClientScene.RegisterSpawnHandler(guid, spawnHandler, unspawnHandler);

            Assert.IsTrue(spawnHandlers.ContainsKey(guid));

            // check spawnHandler above is called
            SpawnHandlerDelegate handler = spawnHandlers[guid];
            handler.Invoke(default);
            Assert.That(handlerCalled, Is.EqualTo(1));
        }

        [Test]
        public void RegisterSpawnHandler_SpawnDelegate_AddsHandlerToSpawnHandlersWithCorrectArguments()
        {
            int handlerCalled = 0;
            Vector3 somePosition = new Vector3(10, 20, 3);

            Guid guid = Guid.NewGuid();
            SpawnDelegate spawnHandler = new SpawnDelegate((pos, assetId) =>
            {
                handlerCalled++;
                Assert.That(pos, Is.EqualTo(somePosition));
                Assert.That(assetId, Is.EqualTo(guid));
                return null;
            });
            UnSpawnDelegate unspawnHandler = new UnSpawnDelegate(x => { });

            ClientScene.RegisterSpawnHandler(guid, spawnHandler, unspawnHandler);

            Assert.IsTrue(spawnHandlers.ContainsKey(guid));

            // check spawnHandler above is called
            SpawnHandlerDelegate handler = spawnHandlers[guid];
            handler.Invoke(new SpawnMessage { position = somePosition, assetId = guid });
            Assert.That(handlerCalled, Is.EqualTo(1));
        }

        [Test]
        public void RegisterSpawnHandler_SpawnDelegate_AddsHandlerToUnSpawnHandlers()
        {
            Guid guid = Guid.NewGuid();
            SpawnDelegate spawnHandler = new SpawnDelegate((x, y) => null);
            UnSpawnDelegate unspawnHandler = new UnSpawnDelegate(x => { });

            ClientScene.RegisterSpawnHandler(guid, spawnHandler, unspawnHandler);

            Assert.IsTrue(unspawnHandlers.ContainsKey(guid));
            Assert.AreEqual(unspawnHandlers[guid], unspawnHandler);
        }

        [Test]
        public void RegisterSpawnHandler_SpawnDelegate_ErrorWhenSpawnHandlerIsNull()
        {
            Guid guid = Guid.NewGuid();
            SpawnDelegate spawnHandler = null;
            UnSpawnDelegate unspawnHandler = new UnSpawnDelegate(x => { });

            LogAssert.Expect(LogType.Error, $"Can not Register null SpawnHandler for {guid}");
            ClientScene.RegisterSpawnHandler(guid, spawnHandler, unspawnHandler);
        }

        [Test]
        public void RegisterSpawnHandler_SpawnDelegate_ErrorWhenUnSpawnHandlerIsNull()
        {
            Guid guid = Guid.NewGuid();
            SpawnDelegate spawnHandler = new SpawnDelegate((x, y) => null);
            UnSpawnDelegate unspawnHandler = null;

            LogAssert.Expect(LogType.Error, $"Can not Register null UnSpawnHandler for {guid}");
            ClientScene.RegisterSpawnHandler(guid, spawnHandler, unspawnHandler);
        }

        [Test]
        public void RegisterSpawnHandler_SpawnDelegate_ErrorWhenAssetIdIsEmpty()
        {
            Guid guid = new Guid();
            SpawnDelegate spawnHandler = new SpawnDelegate((x, y) => null);
            UnSpawnDelegate unspawnHandler = new UnSpawnDelegate(x => { });

            LogAssert.Expect(LogType.Error, "Can not Register SpawnHandler for empty Guid");
            ClientScene.RegisterSpawnHandler(guid, spawnHandler, unspawnHandler);
        }

        [Test]
        public void RegisterSpawnHandler_SpawnDelegate_WarningWhenHandlerForGuidALreadyExists()
        {
            Guid guid = Guid.NewGuid();
            SpawnDelegate spawnHandler = new SpawnDelegate((x, y) => null);
            UnSpawnDelegate unspawnHandler = new UnSpawnDelegate(x => { });

            ClientScene.RegisterSpawnHandler(guid, spawnHandler, unspawnHandler);

            SpawnDelegate spawnHandler2 = new SpawnDelegate((x, y) => new GameObject());
            UnSpawnDelegate unspawnHandler2 = new UnSpawnDelegate(x => UnityEngine.Object.Destroy(x));

            LogAssert.Expect(LogType.Warning, $"Replacing existing spawnHandlers for {guid}.");
            ClientScene.RegisterSpawnHandler(guid, spawnHandler2, unspawnHandler2);
        }


        [Test]
        public void RegisterSpawnHandler_SpawnHandlerDelegate_AddsHandlerToSpawnHandlers()
        {
            Guid guid = Guid.NewGuid();
            SpawnHandlerDelegate spawnHandler = new SpawnHandlerDelegate(x => null);
            UnSpawnDelegate unspawnHandler = new UnSpawnDelegate(x => { });

            ClientScene.RegisterSpawnHandler(guid, spawnHandler, unspawnHandler);

            Assert.IsTrue(spawnHandlers.ContainsKey(guid));
            Assert.AreEqual(spawnHandlers[guid], spawnHandler);
        }

        [Test]
        public void RegisterSpawnHandler_SpawnHandlerDelegate_AddsHandlerToUnSpawnHandlers()
        {
            Guid guid = Guid.NewGuid();
            SpawnHandlerDelegate spawnHandler = new SpawnHandlerDelegate(x => null);
            UnSpawnDelegate unspawnHandler = new UnSpawnDelegate(x => { });

            ClientScene.RegisterSpawnHandler(guid, spawnHandler, unspawnHandler);

            Assert.IsTrue(unspawnHandlers.ContainsKey(guid));
            Assert.AreEqual(unspawnHandlers[guid], unspawnHandler);
        }

        [Test]
        public void RegisterSpawnHandler_SpawnHandlerDelegate_ErrorWhenSpawnHandlerIsNull()
        {
            Guid guid = Guid.NewGuid();
            SpawnHandlerDelegate spawnHandler = null;
            UnSpawnDelegate unspawnHandler = new UnSpawnDelegate(x => { });

            LogAssert.Expect(LogType.Error, $"Can not Register null SpawnHandler for {guid}");
            ClientScene.RegisterSpawnHandler(guid, spawnHandler, unspawnHandler);
        }

        [Test]
        public void RegisterSpawnHandler_SpawnHandlerDelegate_ErrorWhenUnSpawnHandlerIsNull()
        {
            Guid guid = Guid.NewGuid();
            SpawnHandlerDelegate spawnHandler = new SpawnHandlerDelegate(x => null);
            UnSpawnDelegate unspawnHandler = null;

            LogAssert.Expect(LogType.Error, $"Can not Register null UnSpawnHandler for {guid}");
            ClientScene.RegisterSpawnHandler(guid, spawnHandler, unspawnHandler);
        }

        [Test]
        public void RegisterSpawnHandler_SpawnHandlerDelegate_ErrorWhenAssetIdIsEmpty()
        {
            Guid guid = new Guid();
            SpawnHandlerDelegate spawnHandler = new SpawnHandlerDelegate(x => null);
            UnSpawnDelegate unspawnHandler = new UnSpawnDelegate(x => { });

            LogAssert.Expect(LogType.Error, "Can not Register SpawnHandler for empty Guid");
            ClientScene.RegisterSpawnHandler(guid, spawnHandler, unspawnHandler);
        }

        [Test]
        public void RegisterSpawnHandler_SpawnHandlerDelegate_WarningWhenHandlerForGuidALreadyExists()
        {
            Guid guid = Guid.NewGuid();
            SpawnHandlerDelegate spawnHandler = new SpawnHandlerDelegate(x => null);
            UnSpawnDelegate unspawnHandler = new UnSpawnDelegate(x => { });

            ClientScene.RegisterSpawnHandler(guid, spawnHandler, unspawnHandler);

            SpawnHandlerDelegate spawnHandler2 = new SpawnHandlerDelegate(x => new GameObject());
            UnSpawnDelegate unspawnHandler2 = new UnSpawnDelegate(x => UnityEngine.Object.Destroy(x));

            LogAssert.Expect(LogType.Warning, $"Replacing existing spawnHandlers for {guid}.");
            ClientScene.RegisterSpawnHandler(guid, spawnHandler2, unspawnHandler2);
        }


        [Test]
        public void UnregisterSpawnHandler_RemovesSpawnHandlersFromDictionary()
        {
            spawnHandlers.Add(validPrefabGuid, new SpawnHandlerDelegate(x => null));

            ClientScene.UnregisterSpawnHandler(validPrefabGuid);

            Assert.IsFalse(unspawnHandlers.ContainsKey(validPrefabGuid));
        }

        [Test]
        public void UnregisterSpawnHandler_RemovesUnSpawnHandlersFromDictionary()
        {
            unspawnHandlers.Add(validPrefabGuid, new UnSpawnDelegate(x => { }));

            ClientScene.UnregisterSpawnHandler(validPrefabGuid);

            Assert.IsFalse(unspawnHandlers.ContainsKey(validPrefabGuid));
        }

        [Test]
        public void UnregisterSpawnHandler_DoesNotRemovePrefabDictionary()
        {
            prefabs.Add(validPrefabGuid, validPrefab);

            ClientScene.UnregisterSpawnHandler(validPrefabGuid);

            // Should not be removed
            Assert.IsTrue(prefabs.ContainsKey(validPrefabGuid));
        }


        [Test]
        public void ClearSpawners_RemovesAllPrefabsFromDictionary()
        {
            prefabs.Add(Guid.NewGuid(), null);
            prefabs.Add(Guid.NewGuid(), null);
            prefabs.Add(Guid.NewGuid(), null);

            ClientScene.ClearSpawners();

            Assert.IsEmpty(prefabs);
        }

        [Test]
        public void ClearSpawners_RemovesAllSpawnHandlersFromDictionary()
        {
            spawnHandlers.Add(Guid.NewGuid(), null);
            spawnHandlers.Add(Guid.NewGuid(), null);
            spawnHandlers.Add(Guid.NewGuid(), null);

            ClientScene.ClearSpawners();

            Assert.IsEmpty(spawnHandlers);
        }

        [Test]
        public void ClearSpawners_RemovesAllUnspawnHandlersFromDictionary()
        {
            unspawnHandlers.Add(Guid.NewGuid(), null);
            unspawnHandlers.Add(Guid.NewGuid(), null);
            unspawnHandlers.Add(Guid.NewGuid(), null);

            ClientScene.ClearSpawners();

            Assert.IsEmpty(unspawnHandlers);
        }

        [Test]
        public void ClearSpawners_ClearsAllDictionary()
        {
            prefabs.Add(Guid.NewGuid(), null);
            prefabs.Add(Guid.NewGuid(), null);
            prefabs.Add(Guid.NewGuid(), null);

            spawnHandlers.Add(Guid.NewGuid(), null);
            spawnHandlers.Add(Guid.NewGuid(), null);
            spawnHandlers.Add(Guid.NewGuid(), null);

            unspawnHandlers.Add(Guid.NewGuid(), null);
            unspawnHandlers.Add(Guid.NewGuid(), null);
            unspawnHandlers.Add(Guid.NewGuid(), null);

            ClientScene.ClearSpawners();

            Assert.IsEmpty(prefabs);
            Assert.IsEmpty(spawnHandlers);
            Assert.IsEmpty(unspawnHandlers);
        }


        [Test]
        [Ignore("Not Implemented")]
        public void DestroyAllClientObjects()
        {
            // Destroys all NetworkIdentity prefabs in scene
            // Disables all NetworkIdentity scene objects in scene
            // Calls unspawnHandler instead of Destroy/Disable (if one exists)
            ClientScene.DestroyAllClientObjects();
        }



        [Test]
        [Ignore("Not Implemented")]
        public void OnSpawn()
        {
            // Applies Payload Correctly
            // Applies Payload to existing object (if one exists
            // Spawns Prefab from prefab Dictionary
            // Spawns Prefab from Handler
            // Spawns Scene object from spawnableObjects Dictionary
            // gives errors when...
            //   guid and sceneId is empty
            //   cant find prefab/handler with assetId
            //   cant find object with sceneId
            //   failed to spawn prefab
            ClientScene.OnSpawn(new SpawnMessage());
        }
    }
}
