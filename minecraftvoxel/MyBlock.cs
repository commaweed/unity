using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a single minecraft block.
/// </summary>
public class MyBlock {

    private BlockType blockType;
    private bool isSolid;
    private Vector3 position;
    private GameObject parentChunkGameObject;

    /// <summary>
    /// Initialize with the given value.
    /// </summary>
    /// <param name="blockType">The type of block.</param>
    /// <param name="position">The world position of the block.</param>
    /// <param name="parentChunkGameObject">The GameObject of the parent chunk.</param>
    public MyBlock(
        BlockType blockType, 
        Vector3 position, 
        GameObject parentChunkGameObject
    ) {
        this.blockType = blockType;
        this.position = position;
        this.parentChunkGameObject = parentChunkGameObject;
        this.isSolid = this.blockType != BlockType.AIR;
    }

    /// <summary>
    /// Returns the block type.
    /// </summary>
    public BlockType BlockType {
        get { return this.blockType; }
    }

    /// <summary>
    /// Returns the position of this block relative to the world.
    /// </summary>
    public Vector3 Position {
        get { return this.position; }
    }

    /// <summary>
    /// Indicates whether or not this block is solid.  Any block that is not AIR is considered solid
    /// </summary>
    public bool IsSolid {
        get { return this.isSolid; }
    }

    /// <summary>
    /// Create the mesh for the given side of this block.
    /// </summary>
    /// <param name="side">The side for which to create the mesh.</param>
    /// <returns>The newly created Mesh for the given side.</returns>
    private Mesh CreateMesh(CubeSide side) {
        Mesh mesh = new Mesh {
            name = "ScriptedMesh" + side.ToString(),
            vertices = QuadMeshValueUtil.GetVertices(side),
            normals = QuadMeshValueUtil.GetNormals(side),
            uv = MeshUvCache.Instance.GetUvArray(this.blockType, side),
            triangles = QuadMeshValueUtil.Triangles,
        };
        mesh.RecalculateBounds();
        return mesh;
    }

    /// <summary>
    /// Create the GameObject quad for the given side and sets its position relative to the parent.
    /// </summary>
    /// <param name="side">The side for which to create the mesh.</param>
    public void CreateQuad(CubeSide side) {
        Mesh mesh = CreateMesh(side);

        GameObject quad = new GameObject("Quad");
        quad.transform.position = position;
        quad.transform.parent = this.parentChunkGameObject.transform;

        MeshFilter meshFilter = (MeshFilter) quad.AddComponent(typeof(MeshFilter));
        meshFilter.mesh = mesh;
    }

}
