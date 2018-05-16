using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// This is just a test script used on a single block.  It can be deleted at a later date.
/// </summary>
public class CreateQuads : MonoBehaviour {

	public enum BlockType {GRASS, DIRT, STONE};

	public Material cubeMaterial;
	public BlockType bType;

    /// <summary>
    /// Create the Quad for the given side of the block.
    /// </summary>
    /// <param name="side"></param>
	private void CreateQuad(CubeSide side)
	{
		Mesh mesh = new Mesh();
	    mesh.name = "ScriptedMesh" + side.ToString(); 

        Vector2[] test;
		if(bType == BlockType.GRASS && side == CubeSide.TOP) {
            test = MeshUvCache.Instance.GetUvArray((int)ImageBlockIndex.GRASS_TOP);
		} else if(bType == BlockType.GRASS && side == CubeSide.BOTTOM) {
           test = MeshUvCache.Instance.GetUvArray((int)ImageBlockIndex.DIRT);
		} else {
           test = MeshUvCache.Instance.GetUvArray((int)ImageBlockIndex.GRASS_SIDE);
		}

        mesh.vertices = QuadMeshValueUtil.GetVertices(side);
		mesh.normals = QuadMeshValueUtil.GetNormals(side);
		mesh.uv = test;
        mesh.triangles = QuadMeshValueUtil.Triangles;
		 
		mesh.RecalculateBounds();
		
		GameObject quad = new GameObject("Quad");
	    quad.transform.parent = this.gameObject.transform;
     	MeshFilter meshFilter = (MeshFilter) quad.AddComponent(typeof(MeshFilter));
		meshFilter.mesh = mesh;
		MeshRenderer renderer = quad.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
		renderer.material = cubeMaterial;
	}

    /// <summary>
    /// Combine all the quads together into a single cube.
    /// </summary>
	void CombineQuads() {
		
		//1. Combine all children meshes
		MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        int i = 0;
        while (i < meshFilters.Length) {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            i++;
        }

        //2. Create a new mesh on the parent object
        MeshFilter mf = (MeshFilter) this.gameObject.AddComponent(typeof(MeshFilter));
        mf.mesh = new Mesh();

        //3. Add combined meshes on children as the parent's mesh
        mf.mesh.CombineMeshes(combine);

        //4. Create a renderer for the parent
		MeshRenderer renderer = this.gameObject.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
		renderer.material = cubeMaterial;

		//5. Delete all uncombined children
		foreach (Transform quad in this.transform) {
     		Destroy(quad.gameObject);
 		}

	}

    /// <summary>
    /// Create the cube by creating each of the side Quads and then combining them into one Quad.
    /// </summary>
	void CreateCube() {
        foreach (CubeSide side in Enum.GetValues(typeof(CubeSide))) {
            CreateQuad(side);
        }
		CombineQuads();
	}

	// Use this for initialization
	void Start () {
		CreateCube();
	}
}
