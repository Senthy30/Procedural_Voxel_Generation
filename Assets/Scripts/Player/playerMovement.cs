using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerMovement : MonoBehaviour{

    private GameObject mapTerrain;
    private Rigidbody rb;

    private float cooldownJump = 0, cooldownJumpTime = 0.2f;
    private float coolDownForHitTime = 0.15f, coolDownForHit;

    public mapGenerator updateMap;

    public GameObject centerObject;
    public Transform cameraTransform, checkBoxGround;
    public LayerMask groundLayer;

    public int speed;
    public float gravity, jumpBoost;

    private float coolDownForRendererChunck;

    void Start() {
        rb = GetComponent<Rigidbody>();

        mapTerrain = Instantiate(centerObject, transform.position, Quaternion.identity);
        mapTerrain.name = "Terrain";
        mapTerrain.SetActive(true);

        updateMap.updateMapTerrain(mapTerrain, transform.position, false);
    }

    private void Update() {
        if (coolDownForRendererChunck <= 0) {
            if (updateMap.updateMapTerrain(mapTerrain, transform.position, true))
                coolDownForRendererChunck = 0.10f;
        }

        if (coolDownForRendererChunck > 0)
            coolDownForRendererChunck -= Time.deltaTime;
    }

    void FixedUpdate() {
        float xMove = Input.GetAxisRaw("Horizontal");
        float zMove = Input.GetAxisRaw("Vertical");

        rb.velocity = (zMove * transform.forward + xMove * transform.right) * speed * Time.deltaTime + new Vector3(0, rb.velocity.y, 0);

        if (!isGrounded()) {
            //rb.velocity += new Vector3(0, gravity, 0);
        }

        if (Input.GetKey(KeyCode.Space) && cooldownJump <= 0 && isGrounded()) {
            rb.velocity = new Vector3(rb.velocity.x, jumpBoost, rb.velocity.z);
            cooldownJump = cooldownJumpTime;
        }

        if (cooldownJump > 0)
            cooldownJump -= Time.deltaTime;

        if (coolDownForHit <= 0 && Input.GetKey(KeyCode.Mouse0)) {

            RaycastHit hit;
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, 100)) {
                //Debug.Log(hit.point);
                updateMap.destroyBlock(hit.triangleIndex, hit.collider.gameObject);
            }
            coolDownForHit = coolDownForHitTime;

        } else if(coolDownForHit <= 0 && Input.GetKey(KeyCode.Mouse1)) {

            RaycastHit hit;
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, 100)) {
                //Debug.Log(hit.point);
                updateMap.placeBlock(2, hit.triangleIndex, hit.collider.gameObject);
            }
            coolDownForHit = coolDownForHitTime;

        }

        if (coolDownForHit > 0)
            coolDownForHit -= Time.deltaTime;

        //Debug.DrawRay(transform.position, cameraTransform.forward * 100, Color.red, 1);
    }

    bool isGrounded() {
        bool ok = false;
        //ok |= Physics.CheckSphere(checkBoxGround.position, 0.15f, groundLayer);
        ok |= Physics.CheckBox(checkBoxGround.position, new Vector3(0.2f, 0.1f, 0.2f), Quaternion.Euler(Vector3.zero), groundLayer);
        return ok;
    }

}
