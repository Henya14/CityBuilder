using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public struct TreeBatchData
{
    public Mesh mesh;
    public Material material;
    public List<Matrix4x4> matrices;
    public List<TreeInstanceData> treeInstances;

    public TreeBatchData(Mesh mesh, Material material)
    {
        this.mesh = mesh;
        this.material = material;
        this.matrices = new List<Matrix4x4>();
        this.treeInstances = new List<TreeInstanceData>();
    }
}

public struct TreeInstanceData
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
    public Matrix4x4 matrix;

    public TreeInstanceData(Vector3 position, Quaternion rotation, Vector3 scale, Matrix4x4 matrix)
    {
        this.position = position;
        this.rotation = rotation;
        this.scale = scale;
        this.matrix = matrix;
    }
}
public class TreeManager : MonoBehaviour
{

    [SerializeField] List<GameObject> treePrefabs;
    
    [SerializeField] Material treeMaterial;

    [SerializeField] int treeCount;

    [SerializeField] Vector2 treePlacementAreaSize;

    private List<List<TreeBatchData>> batchesForTreeTypes;
    private int batchSize = 1023; // Maximum number of instances per batch for DrawMeshInstanced
    List<Mesh> treeMeshes;

    // Start is called before the first frame update
    void Start()
    {
    }

    public void Init()
    {
         batchesForTreeTypes = new List<List<TreeBatchData>>();
        for (int i = 0; i < treePrefabs.Count; i++)
        {
            batchesForTreeTypes.Add(new List<TreeBatchData>());
        }

        treeMeshes = new List<Mesh>();
        foreach (var prefab in treePrefabs)
        {
            Mesh combinedMesh = MergeMeshes(prefab);
            treeMeshes.Add(combinedMesh);
        }
    }

    public Mesh MergeMeshes(GameObject prefab)
    {
        MeshFilter[] meshFilters = prefab.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        for (int i = 0; i < meshFilters.Length; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
        }
        Mesh mergedMesh = new Mesh();
        mergedMesh.CombineMeshes(combine);
        return mergedMesh;
    }

    public void GenerateTrees()
    {
        foreach (var batch in batchesForTreeTypes)
        {
            batch.Clear();
        }

        for (int i = 0; i < treePrefabs.Count; i++)
        {
            List<TreeInstanceData> treeInstancesToBatch = new List<TreeInstanceData>();
            for (int j = 0; j < treeCount; j++)
            {
                Vector3 position = new Vector3(Random.Range(-treePlacementAreaSize.x / 2, treePlacementAreaSize.x / 2), 0, Random.Range(-treePlacementAreaSize.y / 2, treePlacementAreaSize.y / 2));
                Quaternion rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                Vector3 scale = Vector3.one * Random.Range(0.8f, 1.2f);
                Matrix4x4 matrix = Matrix4x4.TRS(position, rotation, scale);

                treeInstancesToBatch.Add(new TreeInstanceData(position, rotation, scale, matrix));
            }
            batchesForTreeTypes[i] = BatchTrees(treeInstancesToBatch, treeMeshes[i]);
        }
    }

    private List<TreeBatchData> BatchTrees(List<TreeInstanceData> treeInstances, Mesh mesh)
    {
        if (treeInstances.Count == 0) return default;

        Material material = treeMaterial;

        List<TreeBatchData> batches = new List<TreeBatchData>();
        TreeBatchData currentBatch = new TreeBatchData(mesh, material);

        for (int i = 0; i < treeInstances.Count; i++)
        {
            currentBatch.matrices.Add(treeInstances[i].matrix);
            currentBatch.treeInstances.Add(treeInstances[i]);
            if (currentBatch.matrices.Count >= batchSize)
            {
                batches.Add(currentBatch);
                currentBatch = new TreeBatchData(mesh, material);
            }
        }
        if (currentBatch.matrices.Count > 0)
        {
            batches.Add(currentBatch);
        }
        return batches;
    }


    public void RemoveTreesInSquare(Vector3 center, float size)
    {
        for (int i = 0; i < batchesForTreeTypes.Count; i++)
        {
            var treeBatches = batchesForTreeTypes[i];
            var treeInstances = treeBatches.SelectMany(batch => batch.treeInstances).ToList();
            var removed = treeInstances.RemoveAll(tree => Vector3.Distance(tree.position, center) < size / 2);
            batchesForTreeTypes[i] = BatchTrees(treeInstances, treeMeshes[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < batchesForTreeTypes.Count; i++)
        {
            var treeBatches = batchesForTreeTypes[i];
            for (int j = 0; j < treeBatches.Count; j++)
            {
               Graphics.DrawMeshInstanced(treeMeshes[i], 0, treeMaterial, treeBatches[j].matrices);
            }
        }
       
    }
}
