using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
public class BrickPlacementManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Instantiates this prefab on a plane at the touch location.")]
    public GameObject m_PlacedPrefab;

    [SerializeField]
    private GameObject reticle;
    [SerializeField]
    private GameObject blockTemplatePrefab; //dummy, not used. maybe used for later
    [SerializeField]
    private Material templateMaterial; //phantom cube material
    public GameObject[] panels;//blocks panel and colors panel
    public GameObject[] phantomPrefabs;// phantom cubes array
    public GameObject[] blockPrefabs;//AR Placed cubes array
    public Material[] mats;//materials array

    public List<PrefabInfo> newPrefabInfos = new List<PrefabInfo>();//initializing a new list of prefabInfos class
    
    //bools
    private bool buildModeOn = false;
    private bool canBuild = false; //used for instantiating phantom cube
    private bool placeBlock = false;  // used for instantiating placed cube
    public static bool collide = false; // check collision between phantom cube and placed cubes
    private bool noZ = true; // no placing in z direction
    bool[] panelActive = new bool[] { false, false }; // switching on/off panels

    //variables for cube positioning
    private Vector3 buildPos;
    private Quaternion buildRot;
    public float displacement;
    private GameObject currentTemplateBlock;
    GameObject m_PhantomPrefab;//
    Material m_PlacedPrefabMat;
    int objectNum = 0;
    int objectMatNum = 1;

    //AR variables
    ARSessionOrigin m_SessionOrigin;
    Vector2 screenPositionAR;

    private void Awake()
    {
        panels[0].SetActive(false);
        panels[1].SetActive(false);
        m_PhantomPrefab = phantomPrefabs[0];
        m_PlacedPrefab = blockPrefabs[0];
        m_PlacedPrefabMat = mats[1];
    }
    private void Start()
    {
        m_SessionOrigin = GetComponent<ARSessionOrigin>();
        screenPositionAR = new Vector2(Screen.currentResolution.width / 2, Screen.currentResolution.height / 2 + 50);
    }
    
    private void FixedUpdate()
    {
        
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(screenPositionAR);
        if (Physics.Raycast(ray, out hit, 1000f, ~10))
        {
            if(hit.collider.gameObject.tag == "cube1")
            {
                reticle.SetActive(true);
                Vector3 point = hit.point;
                //block placing position in the x direction
                if(Mathf.RoundToInt(hit.normal.x) != 0)
                {
                    buildPos = PhantomPositionXHandler(hit.normal, hit.collider, objectNum);
                }
                //block placing position in the y direction
                if (Mathf.RoundToInt(hit.normal.y) != 0)
                {
                    buildPos = PhantomPositionYHandler(hit.normal, hit.collider, objectNum);
                }

                //so that block can't be placed in z-axis
                if (Mathf.RoundToInt(hit.normal.z) != 0)
                {
                    noZ = false;
                }
                else
                {
                    noZ = true;
                }
                buildRot = hit.transform.rotation;
                canBuild = true;
                //text.text = new Vector3(Mathf.RoundToInt(hit.normal.x) * displacement * 2, Mathf.RoundToInt(hit.normal.y) * displacement, Mathf.RoundToInt(hit.normal.z) * displacement * 2).ToString();
            }
            else
            {
                reticle.SetActive(false);
                Destroy(currentTemplateBlock.gameObject);
                canBuild = false;
            }
        }


        //spawning the phantom cube
        if(canBuild && currentTemplateBlock == null && noZ)
        {
            currentTemplateBlock = Instantiate(m_PhantomPrefab, buildPos, buildRot);
            currentTemplateBlock.GetComponentInChildren<MeshRenderer>().material = templateMaterial;
            currentTemplateBlock.GetComponentInChildren<BoxCollider>().enabled = false;
        }

        //giving position to the phantom cube
        if (canBuild && currentTemplateBlock != null)
        {
            currentTemplateBlock.transform.position = buildPos;
            placeBlock = true;
        }

        //destroying phantom cube when raycast doesn't hit
        if (currentTemplateBlock != null)
        {
            Destroy(currentTemplateBlock.gameObject);
            collide = false;
            canBuild = false;
        }
        
    }

    // phantom cube position in x direction function
    public Vector3 PhantomPositionXHandler(Vector3 pos, Collider obj, int num)
    {
        Vector3 position = new Vector3(Mathf.RoundToInt(pos.x) * displacement * (num + 2), Mathf.RoundToInt(pos.y) * displacement * (num + 1), Mathf.RoundToInt(pos.z) * displacement * (num + 2) * 0) + obj.transform.position + new Vector3(Mathf.RoundToInt(pos.x) * (obj.transform.localScale.x / 2 - displacement), 0, 0) - new Vector3(Mathf.RoundToInt(pos.x) * -0.001f, displacement, 0);
        return position;
    }

    // phantom cube position in y direction function
    public Vector3 PhantomPositionYHandler(Vector3 pos, Collider obj, int num)
    {
        Vector3 position = new Vector3(Mathf.RoundToInt(pos.x) * displacement * 2, Mathf.RoundToInt(pos.y) * displacement + 0.001f, Mathf.RoundToInt(pos.z) * displacement * 2) + obj.transform.position;
        return position;
    }


    //placing block function
    public void PlaceBlockButtonHandler()
    {
        if (reticle.activeSelf && placeBlock && !collide)
        {
            GameObject newBlock = Instantiate(m_PlacedPrefab, buildPos, buildRot);
            AddPrefabListHandler(newPrefabInfos, newBlock, buildPos, buildRot);
            SoundManager.instance.playSFXByID(2);
            placeBlock = false;
        }
        else
        {
            SoundManager.instance.playSFXByID(0);
        }
    }

    //Save function
    public void SaveWallHandler()
    {
        var pc = new PrefabCollection();
        pc.prefabInfos = newPrefabInfos;
        string saveDataString = JsonUtility.ToJson(pc);
        JsonFileUtility.WriteJsonToExternalResource("SaveBrickWall.json", saveDataString);
    }

    //Load function
    public void LoadWallHandler()
    {
        PrefabCollection loadedPrefabInfos = JsonUtility.FromJson<PrefabCollection>(JsonFileUtility.LoadJsonFromFile("SaveBrickWall.json", false));
        foreach (PrefabInfo prefab in loadedPrefabInfos.prefabInfos)
        {
            GameObject loadCube = Instantiate(blockPrefabs[prefab.prefabNum], new Vector3(prefab.posX, prefab.posY, prefab.posZ), new Quaternion(prefab.rotX, prefab.rotY, prefab.rotZ, 0));
            m_SessionOrigin.MakeContentAppearAt(loadCube.transform, loadCube.transform.position, loadCube.transform.rotation);
            loadCube.GetComponentInChildren<MeshRenderer>().material = mats[prefab.matNum];
        }
        SoundManager.instance.playSFXByID(1);
    }

    //change the type of cube to be placed function
    public void PlacedPrefabHandler(int num)
    {
        objectNum = num;
        m_PlacedPrefab = blockPrefabs[num];
        m_PhantomPrefab = phantomPrefabs[num];
        m_PlacedPrefab.GetComponentInChildren<MeshRenderer>().material = m_PlacedPrefabMat;
    }

    //changes the color of the cube to be placed function
    public void ChangePrefabMaterialHandler(int num)
    {
        objectMatNum = num;
        m_PlacedPrefabMat = mats[num];
        m_PlacedPrefab.GetComponentInChildren<MeshRenderer>().material = m_PlacedPrefabMat;
    }

    //switching cubes and colors panel function
    public void PanelActiveHandler(int num)
    {
        panelActive[num] = !panelActive[num];
        panels[num].SetActive(panelActive[num]);
        if (num == 0)
        {
            panelActive[1] = false;
            panels[1].SetActive(false);
        }

        if (num == 1)
        {
            panelActive[0] = false;
            panels[0].SetActive(false);
        }
    }

    //storing placed cube info function
    public void AddPrefabListHandler(List<PrefabInfo> prefabInfos, GameObject cube, Vector3 pos, Quaternion rot)
    {
        PrefabInfo prefabInfo = new PrefabInfo();
        prefabInfo.name = cube.name;
        prefabInfo.prefabNum = objectNum;
        prefabInfo.matNum = objectMatNum;
        prefabInfo.posX = pos.x;
        prefabInfo.posY = pos.y;
        prefabInfo.posZ = pos.z;
        prefabInfo.rotX = rot.x;
        prefabInfo.rotY = rot.y;
        prefabInfo.rotZ = rot.z;

        prefabInfos.Add(prefabInfo);
    }

}


//Creating a class for storing relevant info on a placed cube in order to reload it back in the scene
[System.Serializable]
public class PrefabInfo
{
    public string name;
    public int prefabNum;
    public int matNum;
    public float posX;
    public float posY;
    public float posZ;
    public float rotX;
    public float rotY;
    public float rotZ;
}

//Creating list of prefabinfo class for saving and loading
//used for JSON
[System.Serializable]
public class PrefabCollection
{
    public List<PrefabInfo> prefabInfos;
}

//no overlap between UI and AR Raycast touch position
public static class Vector2Extensions
{
    public static bool IsPointerOverUIObject(this Vector2 pos)
    {
        PointerEventData eventPosition = new PointerEventData(EventSystem.current);
        eventPosition.position = new Vector2(pos.x, pos.y);

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventPosition, results);

        return results.Count > 0;
    }
}
