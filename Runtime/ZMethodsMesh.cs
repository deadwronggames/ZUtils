using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DeadWrongGames.ZUtils
{
    public static class ZMethodsMeshes
    {
        /// <summary>
        /// Combines all active skinned and unskinned meshes into one SkinnedMeshRenderers under the given GameObject.
        /// Deletes all others.
        /// Assumes at least one SkinnedMeshRenderer.
        /// Assumes all use the same skeleton hierarchy and material(s).
        /// </summary>
        public static void CombineAllMeshes(GameObject parentGO)
        {
            SkinnedMeshRenderer[] skinnedRenderers = parentGO.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: false);
            MeshFilter[] filters = parentGO.GetComponentsInChildren<MeshFilter>(includeInactive: false);
            if (!IsCombinationPossible()) return;

            SkinnedMeshRenderer combinedRenderer = skinnedRenderers[0]; // in the end we will use the first one to hold the combined mesh
            List<CombineInstance> combineInstances = new();
            List<BoneWeight> boneWeights = new();
            Matrix4x4[] initialBindposes = combinedRenderer.sharedMesh.bindposes; // all meshes have the same bindposes but the combined mesh has them all duplicated which is not needed. to avoid duplicates, which would also require duplicating bone transforms, just use the initial set 
        
            // Skinned
            foreach (SkinnedMeshRenderer renderer in skinnedRenderers)
            {
                // Add mesh to combine list
                Mesh mesh = UnityEngine.Object.Instantiate(renderer.sharedMesh); // clone to avoid modifying asset TODO necessary?
                combineInstances.Add(new CombineInstance { mesh = mesh, });

                // Copy bone weights
                boneWeights.AddRange(mesh.boneWeights);
            }
        
            // Unskinned
            foreach (MeshFilter filter in filters)
            {
                Mesh mesh = UnityEngine.Object.Instantiate(filter.sharedMesh); // clone to not modify asset
                combineInstances.Add(new CombineInstance { mesh = mesh });

                // Create bone weights
                Transform targetBone = filter.transform.parent;
                int targetBoneIndex = Array.IndexOf(combinedRenderer.bones, targetBone);
                BoneWeight[] weights = new BoneWeight[mesh.vertexCount];
                Array.Fill(weights, new BoneWeight { boneIndex0 = targetBoneIndex, weight0 = 1f });

                // Add bone weights to list
                boneWeights.AddRange(weights);
            }

            // Build final mesh
            Mesh combinedMesh = new() { name = $"{parentGO.name}_CombinedMesh" };
            combinedMesh.CombineMeshes(combineInstances.ToArray(), mergeSubMeshes: true, useMatrices: false);
            combinedMesh.boneWeights = boneWeights.ToArray();
            combinedMesh.bindposes = initialBindposes;
            combinedMesh.RecalculateBounds();
            combinedMesh.Optimize(); // usually done during import but since our mesh is new...
            combinedMesh.UploadMeshData(markNoLongerReadable: true); // could also give some performance

            // Use first renderer for combined mesh
            combinedRenderer.gameObject.name = "CombinedMesh";
            combinedRenderer.sharedMesh = combinedMesh;
        
            // Destroy all original and inactive meshes
            foreach (SkinnedMeshRenderer renderer in parentGO.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true).Where(r => r != skinnedRenderers[0]))
                UnityEngine.Object.Destroy(renderer.gameObject);
            foreach (MeshFilter filter in parentGO.GetComponentsInChildren<MeshFilter>(includeInactive: true))
                UnityEngine.Object.Destroy(filter.gameObject);
        
            return;
            bool IsCombinationPossible()
            {
                string warningMessage = string.Empty;
            
                if (skinnedRenderers.Length == 0) 
                    warningMessage += "No SkinnedMeshRenderers found to combine.\n";
                if (skinnedRenderers.Skip(1).Any(r => { Transform[] bones1; return r.bones.Length != (bones1 = skinnedRenderers[0].bones).Length || !r.bones.SequenceEqual(bones1); }))
                    warningMessage += "Bones do not match.\n";
                if (skinnedRenderers.Skip(1).Any(r => (r.materials.Length != skinnedRenderers[0].materials.Length) || Enumerable.Range(0, r.materials.Length).Any(i => r.materials[i].name != skinnedRenderers[0].materials[i].name)))
                    warningMessage += "Materials do not match.\n";
                if (skinnedRenderers.Skip(1).Any(r => (r.sharedMesh.bindposes.Length != skinnedRenderers[0].sharedMesh.bindposes.Length) || !r.sharedMesh.bindposes.Zip(skinnedRenderers[0].sharedMesh.bindposes, (a, b) => a == b).All(equal => equal)))
                    warningMessage += "Bind poses do not match.\n";
                if(filters.Any(f => !skinnedRenderers[0].bones.Contains(f.transform.parent)))
                    warningMessage += "Unskinned Mesh has no parent bone\n.";
            
                if (string.IsNullOrEmpty(warningMessage)) return true;

                warningMessage = $"{warningMessage}Returning without combining meshes.";
                warningMessage.Log(level: ZMethodsDebug.LogLevel.Error);
                return false;
            }
        }
    }
}
