using System.Collections;
using UnityEngine;

public class EntitiesController : MonoBehaviour {

    /* Variables */
    #region VARIABLES 

    // Serialized Fields
    #region SERIALIZED FIELDS
    [SerializeField] private float spawningSphereRadious = 150, parentRotationSpeed = 0.05f;
    [SerializeField] private GameObject prefab = null;
    [SerializeField, HideInInspector] public int numberOfEntities = 1500;
    #region Hidden Serialized Fields
    [SerializeField, HideInInspector]
    public float SpeedRangeMin = 0.0f, SpeedRangeMax = 5.0f,
    LifeRangeMin = 0.0f, LifeRangeMax = 5.0f,
    ColorRangeMinRed = 0.0f, ColorRangeMaxRed = 1.0f, ColorRangeMinGreen = 0.0f, ColorRangeMaxGreen = 1.0f, ColorRangeMinBlue = 0.0f, ColorRangeMaxBlue = 1.0f;
    [SerializeField, HideInInspector] public int rotationProcessBatch = 500, fadingProcessBatch = 75;
    [SerializeField, HideInInspector] public bool ProcessInBatches = false;
    #endregion
    #endregion

    public IEnumerator rotationCoroutine = null, fadeCoroutine = null;

    private MeshRenderer[] prefabRenderers = null; // if it was a Materials array no run-time, mesh changing would be possible
    private Transform[] transforms = null;
    private Vector3[] angles = null;
    private float[] speeds = null, lifeSpan = null;
    private bool[] needToFade = null;

    private Vector3 parentRotationAngle = Vector3.one;

    #region GET PROPERTIES 
    public Transform[] Transforms => transforms;
    public MeshRenderer[] PrefabsRenderers => prefabRenderers;
    public Vector3[] Angles => angles;
    public float[] Speeds => speeds;
    public float[] LifeSpan => lifeSpan;
    public bool[] NeedToFade => needToFade;
    #endregion
    #endregion
    /* --------- */

    /* Unity Callbacks */
    #region UNITY CALLBACKS

    // Initialise values
    private void Awake() {
        transforms = new Transform[numberOfEntities];
        prefabRenderers = new MeshRenderer[numberOfEntities];
        speeds = new float[numberOfEntities];
        lifeSpan = new float[numberOfEntities];
        angles = new Vector3[numberOfEntities];
        needToFade = new bool[numberOfEntities];
        parentRotationAngle = new Vector3(Random.Range(-1, 2), Random.Range(-1, 2), Random.Range(-1, 2));
        if (parentRotationAngle == Vector3.zero) {
            parentRotationAngle = Vector3.up;
        }
        rotationCoroutine = Rotate();
        fadeCoroutine = Fade();
    }
    // Instansiates GameObjects and starts Rotate() and Fade() coroutines
    void Start() {
        for (int i = 0; i < numberOfEntities; i++) {
            // This allows to change the prefab shape at run-time if mesh-filter's mesh is changed via inspector
            //transforms[i] = ((GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab, this.transform)).transform;
            transforms[i] = Instantiate(prefab, this.transform).transform;
            transforms[i].name += " " + i;
            prefabRenderers[i] = transforms[i].GetComponent<MeshRenderer>();
            InitialisePrefab(i);
        }
        if (ProcessInBatches) {
            StartCoroutine(rotationCoroutine);
            StartCoroutine(fadeCoroutine);
        }
    }

    // Update is called once per frame
    void Update() {
        for (int i = 0; i < numberOfEntities; i++) {
            lifeSpan[i] -= Time.deltaTime;
            if (ProcessInBatches && lifeSpan[i] <= 0) {
                needToFade[i] = true;
            }
            else {
                transform.Rotate(parentRotationAngle, parentRotationSpeed / spawningSphereRadious * Time.deltaTime);
                transforms[i].Rotate(angles[i], speeds[i]);
                if (lifeSpan[i] <= 0) {
                    if (prefabRenderers[i].material.color.a <= 0) {
                        InitialisePrefab(i);
                    }
                    else {
                        Color c = prefabRenderers[i].material.color;
                        c.a -= Time.deltaTime / 10;
                        prefabRenderers[i].material.color = c;
                    }
                }
            }
        }
    }
    #endregion
    /* --------------- */

    /* COROUTINES */
    #region COROUTINES
    /// <summary>Rotates entities in batches </summary>
    private IEnumerator Rotate() {
        int index = 0;
        int batch = rotationProcessBatch;
        while (true) {
            transform.Rotate(parentRotationAngle, parentRotationSpeed / spawningSphereRadious * Time.deltaTime);
            transforms[index].Rotate(angles[index], speeds[index] * (numberOfEntities / rotationProcessBatch));
            index++;

            if (index > batch) {
                yield return new WaitForEndOfFrame();
                batch += rotationProcessBatch;
            }
            if (index >= needToFade.Length) {
                index = 0;
                batch = rotationProcessBatch;
            }

        }

    }
    /// <summary>Decrease color alpha value in batches </summary>
    private IEnumerator Fade() {

        int index = 0;
        int batch = fadingProcessBatch;
        while (true) {

            if (index >= needToFade.Length) {
                index = 0;
                batch = fadingProcessBatch;
            }
            if (needToFade[index] == true) {
                if (prefabRenderers[index].material.color.a <= 0) {
                    InitialisePrefab(index);
                    yield return new WaitForEndOfFrame();
                }
                else {
                    Color c = prefabRenderers[index].material.color;
                    c.a -= 0.1f;
                    prefabRenderers[index].material.color = c;
                }
            }

            index++;
            if (index > batch) {
                yield return new WaitForEndOfFrame();
                batch += fadingProcessBatch;
            }
        }
    }
    #endregion
    /* ---------- */

    /* Private Methods */
    #region PRIVATE METHODS
    private void InitialisePrefab(int index) {
        //Position
        transforms[index].position = Random.insideUnitSphere * spawningSphereRadious;
        // Scale
        transforms[index].localScale = Vector3.one * Random.Range(0.5f, 5);
        // Speed
        speeds[index] = Random.Range(SpeedRangeMin, SpeedRangeMax);
        // Life-span
        lifeSpan[index] = Random.Range(LifeRangeMin, LifeRangeMax);
        // Angle
        angles[index] = new Vector3(Random.Range(-1, 2), Random.Range(-1, 2), Random.Range(-1, 2));
        // Renderer color
        prefabRenderers[index].material.color = new Color(Random.Range(ColorRangeMinRed, ColorRangeMaxRed), Random.Range(ColorRangeMinGreen, ColorRangeMaxGreen), Random.Range(ColorRangeMinBlue, ColorRangeMaxBlue));
        // Reset
        needToFade[index] = false;
    }
    #endregion
    /* --------------- */
}
