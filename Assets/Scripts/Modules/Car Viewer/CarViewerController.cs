using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

/// <summary>
/// Cámara orbital del visualizador 3D: girar (arrastrar), zoom (rueda / pellizco)
/// y enfoque suave de cada CarPart al hacer clic sobre ella o elegirla en la lista.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CarViewerController : MonoBehaviour
{
    [Header("Vehículo")]
    public Transform carRoot;
    public Vector3 overviewPivotOffset = new Vector3(0f, 0.8f, 0f);

    [Header("Vista general")]
    public float overviewYaw = 215f;
    public float overviewPitch = 18f;
    public float overviewDistance = 7.5f;

    [Header("Controles")]
    public float orbitSpeed = 0.25f;
    public float zoomStep = 0.12f;
    public float minDistance = 0.6f;
    public float maxDistance = 14f;
    public float minPitch = -5f;
    public float maxPitch = 80f;
    public float smoothing = 7f;

    public event Action<CarPart> SelectionChanged;
    public IReadOnlyList<CarPart> Parts => parts;
    public CarPart Selected { get; private set; }

    private const float ClickThreshold = 8f;

    private readonly List<CarPart> parts = new List<CarPart>();
    private readonly List<RaycastResult> uiHits = new List<RaycastResult>();
    private Camera cam;
    private CarPart hovered;

    private Vector3 pivot, targetPivot;
    private float yaw, pitch, distance;
    private float targetYaw, targetPitch, targetDistance;

    private bool pressing, pressOverUI, touchActive;
    private float dragAccum, lastPinch;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (carRoot != null) carRoot.GetComponentsInChildren(true, parts);

        SetOverviewTargets();
        pivot = targetPivot;
        yaw = targetYaw;
        pitch = targetPitch;
        distance = targetDistance;
        ApplyCamera();
    }

    private void Update()
    {
        HandleTouch();
        if (!touchActive) HandleMouse();

        Keyboard kb = Keyboard.current;
        if (kb != null && kb.escapeKey.wasPressedThisFrame) ShowOverview();

        float k = 1f - Mathf.Exp(-smoothing * Time.unscaledDeltaTime);
        pivot = Vector3.Lerp(pivot, targetPivot, k);
        yaw = Mathf.Lerp(yaw, targetYaw, k);
        pitch = Mathf.Lerp(pitch, targetPitch, k);
        distance = Mathf.Lerp(distance, targetDistance, k);
        ApplyCamera();
    }

    // ---------- API pública ----------

    public void Select(CarPart part)
    {
        if (part == null) { ShowOverview(); return; }

        if (Selected != null && Selected != part) Selected.SetHighlightLevel(Selected == hovered ? 0.4f : 0f);
        Selected = part;
        part.SetHighlightLevel(1f);

        Bounds b = part.GetWorldBounds();
        targetPivot = b.center;
        float size = Mathf.Max(part.minFocusDistance, b.extents.magnitude * part.distanceMultiplier);
        targetDistance = Mathf.Clamp(size, minDistance, maxDistance);
        SetAngles(part.viewYaw, part.viewPitch);

        SelectionChanged?.Invoke(part);
    }

    public void ShowOverview()
    {
        if (Selected != null) Selected.SetHighlightLevel(Selected == hovered ? 0.4f : 0f);
        Selected = null;
        SetOverviewTargets();
        SelectionChanged?.Invoke(null);
    }

    // ---------- Entrada ----------

    private void HandleMouse()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 pos = mouse.position.ReadValue();
        bool overUI = PointerOverUI(pos);

        if (!pressing) UpdateHover(overUI ? null : RaycastPart(pos));

        if (mouse.leftButton.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame)
        {
            pressing = true;
            pressOverUI = overUI;
            dragAccum = 0f;
        }

        if (pressing && !pressOverUI && (mouse.leftButton.isPressed || mouse.rightButton.isPressed))
        {
            Vector2 d = mouse.delta.ReadValue();
            dragAccum += d.magnitude;
            if (dragAccum > ClickThreshold) Orbit(d);
        }

        if (mouse.leftButton.wasReleasedThisFrame)
        {
            if (pressing && !pressOverUI && dragAccum <= ClickThreshold)
            {
                CarPart p = RaycastPart(pos);
                if (p != null) Select(p);
            }
            pressing = false;
        }
        if (mouse.rightButton.wasReleasedThisFrame) pressing = false;

        float scroll = mouse.scroll.ReadValue().y;
        if (!overUI && Mathf.Abs(scroll) > 0.01f) Zoom(Mathf.Sign(scroll));
    }

    private void HandleTouch()
    {
        Touchscreen ts = Touchscreen.current;
        if (ts == null) return;

        TouchControl t0 = null, t1 = null;
        int count = 0;
        foreach (TouchControl t in ts.touches)
        {
            if (!t.press.isPressed) continue;
            if (count == 0) t0 = t; else if (count == 1) t1 = t;
            count++;
        }

        if (count == 1)
        {
            Vector2 pos = t0.position.ReadValue();
            if (!touchActive)
            {
                touchActive = true;
                pressOverUI = PointerOverUI(pos);
                dragAccum = 0f;
            }
            else if (!pressOverUI)
            {
                Vector2 d = t0.delta.ReadValue();
                dragAccum += d.magnitude;
                if (dragAccum > ClickThreshold * 1.5f) Orbit(d);
            }
        }
        else if (count >= 2)
        {
            touchActive = true;
            dragAccum = float.MaxValue;
            float dist = Vector2.Distance(t0.position.ReadValue(), t1.position.ReadValue());
            if (lastPinch > 0f && dist > 1f)
                targetDistance = Mathf.Clamp(targetDistance * lastPinch / dist, minDistance, maxDistance);
            lastPinch = dist;
        }

        if (count < 2) lastPinch = 0f;

        if (count == 0 && touchActive)
        {
            if (!pressOverUI && dragAccum <= ClickThreshold * 1.5f)
            {
                CarPart p = RaycastPart(ts.primaryTouch.position.ReadValue());
                if (p != null) Select(p);
            }
            touchActive = false;
        }
    }

    private void Orbit(Vector2 delta)
    {
        targetYaw += delta.x * orbitSpeed;
        targetPitch = Mathf.Clamp(targetPitch - delta.y * orbitSpeed, minPitch, maxPitch);
    }

    private void Zoom(float direction)
    {
        targetDistance = Mathf.Clamp(targetDistance * (1f - zoomStep * direction), minDistance, maxDistance);
    }

    private void UpdateHover(CarPart part)
    {
        if (hovered == part) return;
        if (hovered != null && hovered != Selected) hovered.SetHighlightLevel(0f);
        hovered = part;
        if (hovered != null && hovered != Selected) hovered.SetHighlightLevel(0.4f);
    }

    private CarPart RaycastPart(Vector2 screenPos)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);
        return Physics.Raycast(ray, out RaycastHit hit, 200f) ? hit.collider.GetComponentInParent<CarPart>() : null;
    }

    private bool PointerOverUI(Vector2 screenPos)
    {
        EventSystem es = EventSystem.current;
        if (es == null) return false;
        uiHits.Clear();
        es.RaycastAll(new PointerEventData(es) { position = screenPos }, uiHits);
        return uiHits.Count > 0;
    }

    // ---------- Cámara ----------

    private void SetOverviewTargets()
    {
        targetPivot = (carRoot != null ? carRoot.position : Vector3.zero) + overviewPivotOffset;
        targetDistance = overviewDistance;
        SetAngles(overviewYaw, overviewPitch);
    }

    private void SetAngles(float newYaw, float newPitch)
    {
        targetYaw = yaw + Mathf.DeltaAngle(yaw, newYaw);
        targetPitch = Mathf.Clamp(newPitch, minPitch, maxPitch);
    }

    private void ApplyCamera()
    {
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        transform.SetPositionAndRotation(pivot + rot * new Vector3(0f, 0f, -distance), rot);
    }
}
