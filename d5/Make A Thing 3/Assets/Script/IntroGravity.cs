using UnityEngine;
using System.Collections;

public class IntroGravity : MonoBehaviour {

    public Transform centerObj;
    Rigidbody rb;
    Animation animator;

    void Start () {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animation>();
    }

	void FixedUpdate () {
        Vector3 dir = centerObj.transform.position - transform.position;
        dir = dir.normalized;
        rb.AddForce(dir * ((5 * 1) * 1));
        animator.PlayQueued("jumpfall");
    }
}
