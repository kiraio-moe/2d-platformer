using UnityEngine;
using UnityEngine.Events;

public class CharacterController2D : MonoBehaviour
{
	[Tooltip("Amount of force added when the player jumps.")]
	[SerializeField] private float m_JumpForce = 400f;

	[Tooltip("Amount of maxSpeed applied to crouching movement.")]
	[Range(0, 1)] [SerializeField] private float m_CrouchSpeed = .36f;

	[Tooltip("How much to smooth out the movement.")]
	[Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;

	[Tooltip("Whether or not a player can steer while jumping.")]
	[SerializeField] private bool m_AirControl = false;

	[Tooltip("A mask determining what is ground to the character.")]
	[SerializeField] private LayerMask m_WhatIsGround;

	[Tooltip("A position marking where to check if the player is grounded.")]
	[SerializeField] private Transform m_GroundCheck;

	[Tooltip("A position marking where to check for ceilings.")]
	[SerializeField] private Transform m_CeilingCheck;

	[Tooltip("A collider that will be disabled when crouching.")]
	[SerializeField] private Collider2D m_CrouchDisableCollider;

	// Radius of the overlap circle to determine if grounded
	private const float k_GroundedRadius = .2f;

	// Whether or not the player is grounded.
	private bool m_Grounded;

	// Radius of the overlap circle to determine if the player can stand up
	private const float k_CeilingRadius = .2f;

	// For determining which way the player is currently facing.
	private bool m_FacingRight = true;

	private Vector3 m_Velocity = Vector3.zero;

	private Rigidbody2D m_Rigidbody2D;

	private SpriteRenderer m_SpriteRenderer;

	[Header("Events")]
	[Space]

	[SerializeField] public UnityEvent OnLandEvent;

	[System.Serializable]
	public class BoolEvent : UnityEvent<bool> {}

	[SerializeField] public BoolEvent OnCrouchEvent;
	private bool m_wasCrouching = false;

	private void Awake() {
		m_Rigidbody2D = GetComponent<Rigidbody2D>();
		m_SpriteRenderer = GetComponent<SpriteRenderer>();

		if (OnLandEvent == null)
			OnLandEvent = new UnityEvent();

		if (OnCrouchEvent == null)
			OnCrouchEvent = new BoolEvent();
	}

	private void FixedUpdate() {
		bool wasGrounded = m_Grounded;
		m_Grounded = false;

		// The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
		// This can be done using layers instead but Sample Assets will not overwrite your project settings.
		Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);

		for (int i = 0; i < colliders.Length; i++)
		{
			if (colliders[i].gameObject != gameObject)
			{
				m_Grounded = true;
				if (!wasGrounded)
					OnLandEvent.Invoke();
			}
		}
	}

	public void Move(float m_Move, bool m_Crouch, bool m_Jump) {
		// If crouching, check to see if the character can stand up
		if (!m_Crouch) {
			// If the character has a ceiling preventing them from standing up, keep them crouching
			if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround)) {
				m_Crouch = true;
			}
		}

		//only control the player if grounded or airControl is turned on
		if (m_Grounded || m_AirControl) {

			// If crouching
			if (m_Crouch) {
				if (!m_wasCrouching) {
					m_wasCrouching = true;
					OnCrouchEvent.Invoke(true);
				}

				// Reduce the speed by the crouchSpeed multiplier
				m_Move *= m_CrouchSpeed;

				// Disable one of the colliders when crouching
				if (m_CrouchDisableCollider != null) {
					m_CrouchDisableCollider.enabled = false;
				}
			} else {
				// Enable the collider when not crouching
				if (m_CrouchDisableCollider != null) {
					m_CrouchDisableCollider.enabled = true;
				}

				if (m_wasCrouching) {
					m_wasCrouching = false;
					OnCrouchEvent.Invoke(false);
				}
			}

			// Move the character by finding the target velocity
			Vector3 targetVelocity = new Vector2(m_Move * 10f, m_Rigidbody2D.velocity.y);
			// And then smoothing it out and applying it to the character
			m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);

			// If the input is moving the player right and the player is facing left...
			if (m_Move > 0 && !m_FacingRight) {
				// ... flip the player.
				Flip();
			} else if (m_Move < 0 && m_FacingRight) {
				// Otherwise if the input is moving the player left and the player is facing right...
				// ... flip the player.
				Flip();
			}
		}
		// If the player should jump...
		if (m_Grounded && m_Jump) {
			// Add a vertical force to the player.
			m_Grounded = false;
			m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
		}
	}

	private void Flip() {
		// Switch the way the player is labelled as facing.
		m_FacingRight = !m_FacingRight;

		// Multiply the player's x local scale by -1.
		// Vector3 theScale = transform.localScale;
		// theScale.x *= -1;
		// transform.localScale = theScale;

		// Alternative way by flipping the sprite x axis
		if (m_FacingRight) {
			m_SpriteRenderer.flipX = false;
		} else {
			m_SpriteRenderer.flipX = true;
		}
	}
}
