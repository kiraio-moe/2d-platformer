using UnityEngine;

public class PlayerMovement : MonoBehaviour {
	[Tooltip("Character movement speed.")]
	[SerializeField] public float m_RunSpeed = 40f;

	private CharacterController2D m_Controller;
	private Animator m_Animator;
	private float m_HorizontalMove = 0f;
	private bool m_Jump = false;
	private bool m_Crouch = false;

	private void Awake() {
		m_Controller = gameObject.GetComponent<CharacterController2D>();
		m_Animator = gameObject.GetComponent<Animator>();
	}

	public void OnLanding() {
		// Set character jumping animation to false
		m_Animator.SetBool("IsJumping", false);
	}

	public void OnCrouching(bool m_isCrouching) {
		m_Animator.SetBool("IsCrouching", m_isCrouching);
	}

	private void Update() {
		m_HorizontalMove = Input.GetAxisRaw("Horizontal") * m_RunSpeed;

		m_Animator.SetFloat("Speed", Mathf.Abs(m_HorizontalMove));

		if (Input.GetButtonDown("Jump")) {
			m_Jump = true;
			m_Animator.SetBool("IsJumping", true);
		}

		if (Input.GetButtonDown("Crouch")) {
			m_Crouch = true;
		} else if (Input.GetButtonUp("Crouch")) {
			m_Crouch = false;
		}

		OnCrouching(m_Crouch);
	}

	private void FixedUpdate() {
		// Move our character
		m_Controller.Move(m_HorizontalMove * Time.fixedDeltaTime, m_Crouch, m_Jump);

		m_Jump = false;
	}
}
