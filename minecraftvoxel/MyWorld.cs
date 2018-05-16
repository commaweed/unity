using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// Generates the minecraft world and renders it to the scene.
/// </summary>
public class MyWorld : MonoBehaviour {

    [SerializeField]
    private GameObject player;

    [SerializeField]
    private GameObject loadingBarPanel;

    [SerializeField]
    private Text loadingPercentageText;

    [SerializeField]
    private Slider loadingBar;

    [SerializeField]
    private Camera loadScreenCamera;

    [SerializeField]
    private Material minecraftMaterial;  // the blockatlas image that will be subdivided into a grid

    private static readonly int PLAYER_CHUNK_RADIUS = 3;

    /// <summary>
    /// The number of chunks to stack on top of each other.
    /// </summary>
    public static readonly int CHUNK_COLUMN_SIZE = 16;

    /// <summary>
    /// The chunk size to use.  Our chunks are cubes.  Do not go higher than 16.
    /// </summary>
    public static readonly int CHUNK_SIZE = 16;

    private static readonly float TOTAL_BUILD_UNITS = (Mathf.Pow(PLAYER_CHUNK_RADIUS * 2 + 1, 2) * CHUNK_COLUMN_SIZE) / 4f +
         (Mathf.Pow(PLAYER_CHUNK_RADIUS * 2 + 1, 2));

    private int buildProgressUnitCounter = 0;

    private IEnumerator updateProgressBar() {
        this.loadingBar.value = ++this.buildProgressUnitCounter / TOTAL_BUILD_UNITS * 100;
        this.loadingPercentageText.text = (int) this.loadingBar.value + "%";
        yield return null;
    }

    private static Dictionary<Vector3, MyChunk> chunkCache = new Dictionary<Vector3, MyChunk>();

    /// <summary>
    /// Builds the chunk cache.  At the moment, it only builds a single chunk column.
    /// </summary>
    private IEnumerator BuildChunkCacheAsync() {
   
        int playerX = (int) Mathf.Floor(player.transform.position.x / CHUNK_SIZE);
        int playerZ = (int) Mathf.Floor(player.transform.position.z / CHUNK_SIZE);

        for (int z = -PLAYER_CHUNK_RADIUS; z <= PLAYER_CHUNK_RADIUS; z++) {        
            for (int x = -PLAYER_CHUNK_RADIUS; x <= PLAYER_CHUNK_RADIUS; x++) {
                yield return updateProgressBar();
                for (int y = 0; y < CHUNK_COLUMN_SIZE; y++) {
                    Vector3 position = new Vector3(
                        (x + playerX) * CHUNK_SIZE,
                        y * CHUNK_SIZE,
                        (z + playerZ) * CHUNK_SIZE
                    );
                    MyChunk chunk = new MyChunk(CHUNK_SIZE, position, minecraftMaterial);
                    chunk.ChunkGameObject.transform.parent = this.transform;
                    chunkCache.Add(position, chunk);    
                }     
            } 
        } 
    }

    /// <summary>
    /// Renders the world to the scene.
    /// </summary>
    /// <returns></returns>
    private IEnumerator RenderWorldAsync() {
        int counter = 0;
        foreach (KeyValuePair<Vector3, MyChunk> cacheEntry in chunkCache) {
            RenderChunk(cacheEntry.Value);
            if (++counter % 4 == 0) {
                yield return updateProgressBar();
            }
        }
        player.SetActive(true);
    }

    /// <summary>
    /// Loads the world asynchronously.
    /// </summary>
    /// <returns></returns>
    private IEnumerator LoadMyWorldAsync() {
        yield return BuildChunkCacheAsync();
        yield return RenderWorldAsync();

        Canvas mainCanvas = FindObjectOfType<Canvas>();
        mainCanvas.gameObject.SetActive(false); // TODO: destroy it
        loadScreenCamera.gameObject.SetActive(false); // TODO: destroy it

        player.SetActive(true);
    }

    /// <summary>
    /// Renders a chunk to the scene.
    /// </summary>
    public void RenderChunk(MyChunk chunk) {
        for (int z = 0; z < CHUNK_SIZE; z++) {
            for (int y = 0; y < CHUNK_SIZE; y++) {
                for (int x = 0; x < CHUNK_SIZE; x++) {
                    this.RenderBlock(chunk, x, y, z);
                }
            }
        }

        CombineChunkMeshes(chunk);

        MeshCollider collider = chunk.ChunkGameObject.AddComponent(typeof(MeshCollider)) as MeshCollider;
        collider.sharedMesh = chunk.ChunkGameObject.transform.GetComponent<MeshFilter>().mesh;
    }

    /// <summary>
    /// Creates the given block by constructing the Meshes that will be rendered, but only for the sides that
    /// have a solid neighbor.
    /// </summary>
    /// <param name="chunk">The parent chunk the block is in.</param>
    /// <param name="x">The x-coordinate position of the block to render.</param>
    /// <param name="y">The y-coordinate position of the block to render.</param>
    /// <param name="z">The z-coordinate position of the block to render.</param>
    private void RenderBlock(MyChunk chunk, int x, int y, int z) {
        MyBlock block = chunk.ChunkMetadata[x, y, z];
        if (block.BlockType == BlockType.AIR) return;

        foreach (CubeSide side in Enum.GetValues(typeof(CubeSide))) {
            if (!IsNeighboringBlockSolid(chunk, side, x, y, z)) {
                block.CreateQuad(side);
            }
        }
    }

    /// <summary>
    /// Indicates whether or not the given the block in the given chunk at the given coordinates has a neighbor that
    /// is solid on the given side.  It also will check neighboring chunks and do the same. 
    /// </summary>
    /// <param name="chunk">The parent chunk the block is in.</param>
    /// <param name="side">The side to test</param>
    /// <param name="x">The x-coordinate position of the block in the chunk.</param>
    /// <param name="y">The y-coordinate position of the block in the chunk.</param>
    /// <param name="z">The z-coordinate position of the block in the chunk.</param>
    /// <returns>true if the neighbor on the given side exists and is not AIR.</returns>
    private bool IsNeighboringBlockSolid(MyChunk chunk, CubeSide side, int x, int y, int z) {
        bool result = false;

        // if neighboring block is in the given chunk and is solid, then true
        Vector3 nextBlockPosition = new Vector3(x, y, z);
        Vector3 nextChunkPosition = chunk.ChunkGameObject.transform.position; // first set to current chunk
        Boolean isNextBlockInChunk = false;
        switch (side) {
            case CubeSide.BACK:
                nextBlockPosition += Vector3.back;
                isNextBlockInChunk = nextBlockPosition.z >= 0;
                nextChunkPosition.z -= CHUNK_SIZE;
                break;
            case CubeSide.BOTTOM:
                nextBlockPosition += Vector3.down;
                isNextBlockInChunk = nextBlockPosition.y >= 0;
                nextChunkPosition.y -= CHUNK_SIZE;
                break;
            case CubeSide.LEFT:
                nextBlockPosition += Vector3.left;
                isNextBlockInChunk = nextBlockPosition.x >= 0;
                nextChunkPosition.x -= CHUNK_SIZE;
                break;
            case CubeSide.FRONT:
                nextBlockPosition += Vector3.forward;
                isNextBlockInChunk = nextBlockPosition.z < CHUNK_SIZE;
                nextChunkPosition.z += CHUNK_SIZE;
                break;
            case CubeSide.TOP:
                nextBlockPosition += Vector3.up;
                isNextBlockInChunk = nextBlockPosition.y < CHUNK_SIZE;
                nextChunkPosition.y += CHUNK_SIZE;
                break;
            case CubeSide.RIGHT:
                nextBlockPosition += Vector3.right;
                isNextBlockInChunk = nextBlockPosition.x < CHUNK_SIZE;
                nextChunkPosition.x += CHUNK_SIZE;
                break;
        }

        result = isNextBlockInChunk && chunk.ChunkMetadata[
            (int) nextBlockPosition.x,
            (int) nextBlockPosition.y,
            (int) nextBlockPosition.z
        ].IsSolid;

        // if neighbor not in chunk, check neighboring chunk to see if it is solid
        if (!isNextBlockInChunk) {
            int rolloverValue = CHUNK_SIZE - 1;
            MyChunk nextChunk = null;
            bool foundNextChunk = chunkCache.TryGetValue(nextChunkPosition, out nextChunk);
            if (foundNextChunk) {
                // translate next block location to new location in the next chunk
                // modding takes care of upper bound (lower bound needs to be set to 0)
                int newX = (int) nextBlockPosition.x % CHUNK_SIZE;
                int newY = (int) nextBlockPosition.y % CHUNK_SIZE;
                int newZ = (int) nextBlockPosition.z % CHUNK_SIZE;
                result = nextChunk.ChunkMetadata[
                    newX < 0 ? rolloverValue : newX,
                    newY < 0 ? rolloverValue : newY,
                    newZ < 0 ? rolloverValue : newZ
                ].IsSolid;
            }
        }

        return result;
    }

    /// <summary>
    /// Combine all the chunk meshes into a single mesh.
    /// </summary>
    void CombineChunkMeshes(MyChunk chunk) {
        //1. Combine all children meshes
        MeshFilter[] meshFilters = chunk.ChunkGameObject.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        int i = 0;
        while (i < meshFilters.Length) {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            i++;
        }

        //2. Create a new mesh on the parent object
        MeshFilter mf = (MeshFilter)chunk.ChunkGameObject.AddComponent(typeof(MeshFilter));
        mf.mesh = new Mesh();

        //3. Add combined meshes on children as the parent's mesh
        mf.mesh.CombineMeshes(combine);

        //4. Create a renderer for the parent
        MeshRenderer renderer = chunk.ChunkGameObject.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
        renderer.material = minecraftMaterial;

        //5. Delete all uncombined children
        foreach (Transform quad in chunk.ChunkGameObject.transform) {
            GameObject.Destroy(quad.gameObject);
        }
    }

    /// <summary>
    /// Build the chunk cache; does not render anything.
    /// </summary>
    private void Awake() {
        if (player == null) {
            throw new System.ArgumentException("Missing player; did you forget to add it to the MyWorld Prefab?");
        }
        player.transform.position = new Vector3(0, NoiseUtil.MAX_TERRAIN_HEIGHT + 1, 0);
        player.SetActive(false);
    }

    public void RenderWorld() {
        this.loadingBarPanel.SetActive(true);
        resetWorldPosition();
        StartCoroutine(LoadMyWorldAsync());
    }

    /// <summary>
    ///  set the position and rotation of the world to origin and none.
    /// </summary>
    private void resetWorldPosition() {
        this.transform.position = Vector3.zero;
        this.transform.rotation = Quaternion.identity;
    }

    // Use this for initialization
    void Start() {
    }

}
