using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Variables;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

[Version(1, 0, 0)]
[Title("Vision Cone Detection")]
[Category("AI/Vision Cone Detection")]
[Description("Triggered when a vision cone detects a specific tag")]

[Image(typeof(IconEye), ColorTheme.Type.Yellow)]

[Keywords("Sight", "Vision", "Detect", "Tag")]

[Serializable]
public class AIVison : GameCreator.Runtime.VisualScripting.Event
{
    //[SerializeField] private PropertyGetBool m_Value;
    [SerializeField] private PropertyTypeGetBool m_Canee;
    [SerializeField] private PropertySetBool m_CanSee = SetBoolLocalName.Create;
    //[SerializeField] public bool m_CanSee;
    [SerializeField] private string Tag = "Player";
    [SerializeField] private Material VisionConeMaterial;
    [SerializeField] private float VisionRange = 6;
    [SerializeField] private float VisionAngle = 1.5f;
    [SerializeField] private LayerMask VisionObstructingLayer;
    [SerializeField] private int VisionConeResolution = 120;
    [SerializeField] private float VisionHeight = 0.04f;
    private bool CanSee =false;
    private Mesh VisionConeMesh;
    private MeshFilter MeshFilter_;

    protected override void OnStart(Trigger trigger)
    {
        base.OnStart(this.m_Trigger);
        this.m_Trigger.gameObject.AddComponent<MeshRenderer>().material = VisionConeMaterial;
        MeshFilter_ = this.m_Trigger.gameObject.AddComponent<MeshFilter>();
        VisionConeMesh = new Mesh();
    }

    protected override void OnUpdate(Trigger trigger)
    {
        base.OnUpdate(this.m_Trigger);
        DrawVisionCone(this.m_Trigger);
    }

    private void DrawVisionCone(Trigger trigger)
    {
        int[] triangles = new int[(VisionConeResolution - 1) * 3];
        Vector3[] vertices = new Vector3[VisionConeResolution + 1];
        vertices[0] = new Vector3(0, VisionHeight, 0); // Set the origin at the specified height

        float currentAngle = -VisionAngle / 2;
        float angleIncrement = VisionAngle / (VisionConeResolution - 1);
        float sine, cosine;

        for (int i = 0; i < VisionConeResolution; i++)
        {
            sine = Mathf.Sin(currentAngle);
            cosine = Mathf.Cos(currentAngle);
            Vector3 raycastDirection = (trigger.transform.forward * cosine) + (trigger.transform.right * sine);
            Vector3 vertForward = (Vector3.forward * cosine) + (Vector3.right * sine);

            if (Physics.Raycast(trigger.transform.position + Vector3.up * VisionHeight, raycastDirection, out RaycastHit hit, VisionRange, VisionObstructingLayer))
            {
                vertices[i + 1] = vertForward * hit.distance + Vector3.up * VisionHeight;

                if (hit.collider.gameObject.CompareTag(Tag))
                {
                    Debug.Log(Tag + " Detected!");
                    //m_CanSee.Get(trigger);
                    this.m_CanSee.Set(true, this.m_Trigger);
                    // You can add more logic here, for example, triggering other Game Creator actions
                }
            }
            else
            {
               // m_Value.
                this.m_CanSee.Set(false, this.m_Trigger);
                vertices[i + 1] = vertForward * VisionRange + Vector3.up * VisionHeight;
            }

            currentAngle += angleIncrement;
        }

        for (int i = 0, j = 0; i < triangles.Length; i += 3, j++)
        {
            triangles[i] = 0;
            triangles[i + 1] = j + 1;
            triangles[i + 2] = j + 2;
        }

        VisionConeMesh.Clear();
        VisionConeMesh.vertices = vertices;
        VisionConeMesh.triangles = triangles;
        MeshFilter_.mesh = VisionConeMesh;
    }
}

