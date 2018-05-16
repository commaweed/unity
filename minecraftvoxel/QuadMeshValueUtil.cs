using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Represents the quad values that will be used to populate one side of the minecraft block Mesh.
/// The UV array values will need to be derived from the cache.
/// Many of the underlying values can be reused for any block.
/// </summary>
public static class QuadMeshValueUtil {

    public static readonly int VERTICES_SIZE = 4;
    public static readonly int NORMALS_SIZE = 4;
    public static readonly int TRIANGLES_SIZE = 6;
    public static readonly int UVS_SIZE = 4;

    /// <summary>
    /// Represents the Triangles array for any side of a block.  It will be used to build the mesh for a
    /// particular side of a block.  All sides will use the same values.
    /// </summary>
    public static readonly int[] Triangles = new int[] { 3, 1, 0, 3, 2, 1 };

    private static readonly Vector3[] VERTICES = {
        new Vector3(-0.5f, -0.5f, 0.5f),
        new Vector3(0.5f, -0.5f, 0.5f),
        new Vector3(0.5f, -0.5f, -0.5f),
        new Vector3(-0.5f, -0.5f, -0.5f),
        new Vector3(-0.5f, 0.5f, 0.5f),
        new Vector3(0.5f, 0.5f, 0.5f),
        new Vector3(0.5f, 0.5f, -0.5f),
        new Vector3(-0.5f, 0.5f, -0.5f)
    };

    // order is important (according to CubeSide order)
    private static readonly Vector3[][] SIDE_VERTICES = {
        new Vector3[] { VERTICES[0], VERTICES[1], VERTICES[2], VERTICES[3] },
        new Vector3[] { VERTICES[7], VERTICES[6], VERTICES[5], VERTICES[4] },
        new Vector3[] { VERTICES[7], VERTICES[4], VERTICES[0], VERTICES[3] },
        new Vector3[] { VERTICES[5], VERTICES[6], VERTICES[2], VERTICES[1] },
        new Vector3[] { VERTICES[4], VERTICES[5], VERTICES[1], VERTICES[0] },
        new Vector3[] { VERTICES[6], VERTICES[7], VERTICES[3], VERTICES[2] }
    };

    // order is important (according to CubeSide order)
    private static readonly Vector3[][] SIDE_NORMALS = {
        new Vector3[] { Vector3.down, Vector3.down, Vector3.down, Vector3.down },
        new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up },
        new Vector3[] { Vector3.left, Vector3.left, Vector3.left, Vector3.left },
        new Vector3[] { Vector3.right, Vector3.right, Vector3.right, Vector3.right },
        new Vector3[] { Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward },
        new Vector3[] { Vector3.back, Vector3.back, Vector3.back, Vector3.back }
    };

    /// <summary>
    /// Returns the vertices array for the given side.
    /// </summary>
    /// <param name="cubeSide">The side of the cube.</param>
    /// <returns>A vertices array that can be used to create the block Mesh for the given block side.</returns>
    public static Vector3[] GetVertices(CubeSide cubeSide) {
        return SIDE_VERTICES[(int) cubeSide];
    }

    /// <summary>
    /// Returns the normals array for the given side.
    /// </summary>
    /// <param name="cubeSide">The side of the cube.</param>
    /// <returns>A normals array that can be used to create the block Mesh for the given block side.</returns>
    public static Vector3[] GetNormals(CubeSide cubeSide) {
        return SIDE_NORMALS[(int)cubeSide];
    }

}
