using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserPointer : MonoBehaviour {

    private SteamVR_TrackedObject trackedObj;
    public GameObject laserPrefab;
    private GameObject laser;
    private Transform laserTransform;
    private Vector3 hitPoint;

    public Transform cameraRigTransform;
    public GameObject teleportReticlePrefab;
    private GameObject reticle;
    private Transform teleportReticleTransform;
    public Transform headTransform;
    public Vector3 teleportReticleOffset;
    public LayerMask teleportMask;
    private bool shouldTeleport;

    #region Members
    private SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    } 
    #endregion

    #region Unity Overrides
    void Awake()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
    }

    private void Start()
    {
        laser = Instantiate(laserPrefab);
        laserTransform = laser.transform;
        reticle = Instantiate(teleportReticlePrefab);
        teleportReticleTransform = reticle.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (Controller.GetPress(SteamVR_Controller.ButtonMask.Touchpad))
        {
            RaycastHit hit;

            if (Physics.Raycast(trackedObj.transform.position, transform.forward, out hit, 100, teleportMask))
            {
                hitPoint = hit.point;
                ShowLaser(hit);

                reticle.SetActive(true);
                teleportReticleTransform.position = hitPoint + teleportReticleOffset;
                shouldTeleport = true;
            }
        }
        else
        {
            laser.SetActive(false);
            reticle.SetActive(false);
        }

        if (Controller.GetPressUp(SteamVR_Controller.ButtonMask.Touchpad) && shouldTeleport)
        {
            Teleport();
        }
    } 
    #endregion

    private void ShowLaser(RaycastHit hit)
    {
        laser.SetActive(true);
        laserTransform.position = Vector3.Lerp(trackedObj.transform.position, hitPoint, .5f);
        laserTransform.LookAt(hitPoint);
        laserTransform.localScale = new Vector3(laserTransform.localScale.x, laserTransform.localScale.y,
            hit.distance);
    }

    private void Teleport()
    {
        shouldTeleport = false;
        reticle.SetActive(false);
        Vector3 difference = cameraRigTransform.position - headTransform.position;
        difference.y = 0;
        cameraRigTransform.position = hitPoint + difference;
    }

    public static void GetArcHits(out List<RaycastHit2D> Hits, out List<Vector3> Points, int iLayerMask, Vector3 vStart, Vector3 vVelocity, Vector3 vAcceleration, float fTimeStep = 0.05f, float fMaxtime = 10f, bool bDebugDraw = false)
    {
        Hits = new List<RaycastHit2D>();
        Points = new List<Vector3>();
        Vector3 prev = vStart;
        Points.Add(vStart);
        for (int i = 1; ; i++)
        {
            float t = fTimeStep * i;
            if (t > fMaxtime) break;
            Vector3 pos = PlotTrajectoryAtTime(vStart, vVelocity, vAcceleration, t);
            var result = Physics2D.Linecast(prev, pos, iLayerMask);
            if (result.collider != null)
            {
                Hits.Add(result);
                Points.Add(pos);
                break;
            }
            else
            {
                Points.Add(pos);
            }
            Debug.DrawLine(prev, pos, Color.Lerp(Color.yellow, Color.red, 0.35f), 0.5f);
            prev = pos;
        }
    }

    public static Vector3 PlotTrajectoryAtTime(Vector3 start, Vector3 startVelocity, Vector3 acceleration, float fTimeSinceStart)
    {
        return start + startVelocity * fTimeSinceStart + acceleration * fTimeSinceStart * fTimeSinceStart * 0.5f;
    }
}
