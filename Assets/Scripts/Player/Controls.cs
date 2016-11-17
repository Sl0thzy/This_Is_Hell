using UnityEngine;
using System.Collections;

[RequireComponent (typeof(PlayerController))]
[RequireComponent(typeof(GunController))]
public class Controls : LivingEntity {

    public float moveSpeed = 5f;
    PlayerController player;
    public Crosshair crosshair;
    Camera viewCamera;
    GunController gunContoller;

	
	protected override void Start () {
        base.Start();
        player = GetComponent<PlayerController>();
        gunContoller = GetComponent<GunController>();
        viewCamera = Camera.main;
        gunContoller.EquipGun(0);
	}
	
	
	void Update () {

        // Movement Input
        Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")); // WASD Input Get
        Vector3 moveVelocity = moveInput.normalized * moveSpeed; // Move player based on input
        player.Move(moveVelocity);

        // Look Input
        Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.up * gunContoller.GunHeight);
        float rayDistance;

        if (groundPlane.Raycast(ray, out rayDistance))
        {
            Vector3 point = ray.GetPoint(rayDistance);
            player.LookAt(point);
            crosshair.transform.position = point; // Crosshair pos = mouse point
            crosshair.DetectTargets(ray);
            if (((new Vector2(point.x, point.z) - new Vector2(transform.position.x, transform.position.z)).sqrMagnitude) > 1)
            {
                gunContoller.Aim(point);
            }
        //    print ((new Vector2(point.x, point.z) - new Vector2(transform.position.x, transform.position.z)).magnitude);
        }

        // Weapon Input
        if (Input.GetMouseButtonDown(0))
            {
                gunContoller.OnTriggerHold();
            }

        if (Input.GetMouseButtonUp(0))
            {
                gunContoller.OnTriggerRelease();
            }

        if (Input.GetKeyDown(KeyCode.R))
        {
            gunContoller.Reload();
        }
    }

    public override void Die()
    {
        AudioManager.instance.PlaySound("Player Death", transform.position);
        base.Die();
    }
}
