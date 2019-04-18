﻿namespace Game
{
    using UnityEngine;
    using UnityEngine.SceneManagement;

// [ ] Barra de vida
// [ ] Numero de itens coletados
// [ ] Numero de itens totais
// [ ] Elemento de imagem representando vitória / derrota
// [ ] Botão para reiniciar o jogo
// [ ] Menu inicial para o jogo
// [ ] Menu de pausa

    public class PlayerController : MonoBehaviour
    {
        private static readonly int Moving = Animator.StringToHash("Moving");
        private static readonly int Jumping = Animator.StringToHash("Jumping");
        private static readonly int VerticalVelocity = Animator.StringToHash("VerticalVelocity");

        private static readonly float upThreshold = 0.5f;

        private Rigidbody2D rigidBody;

        private SpriteRenderer spriteRenderer;

        private Animator animator;

        public float acceleration = 5;
        public float maxVelocity = 5;

        public float jumpSpeed = 5;

        private Vector2 playerInput;

        public bool grounded;
        public bool canJump;
        public float timeSinceJump = 0;

        void Awake()
        {
            rigidBody = GetComponent<Rigidbody2D>();

            spriteRenderer = GetComponent<SpriteRenderer>();

            animator = GetComponent<Animator>();
        }

        void Update()
        {
            playerInput.x = Input.GetAxisRaw("Horizontal");
            playerInput.y = Input.GetAxisRaw("Vertical");
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            MovementLogic();
        }

        private void MovementLogic()
        {
            var hForce = playerInput.x * acceleration;

            if (!grounded)
            {
                timeSinceJump += Time.fixedDeltaTime;
            }

            if (Mathf.Approximately(hForce, 0))
            {
                hForce = -rigidBody.velocity.x;
            }

            rigidBody.AddForce(new Vector2(hForce, 0), ForceMode2D.Impulse);

            if (!Mathf.Approximately(playerInput.x, 0))
            {
                spriteRenderer.flipX = playerInput.x < 0;
            }

            var vel = rigidBody.velocity;

            if (canJump && playerInput.y > 0)
            {
                vel.y = jumpSpeed;
                grounded = false;
                canJump = false;
            }

            vel.x = Mathf.Clamp(vel.x, -maxVelocity, maxVelocity);
            rigidBody.velocity = vel;

            animator.SetBool(Moving, Mathf.Abs(vel.x) > 0.1f);
            animator.SetBool(Jumping, !grounded);
            animator.SetFloat(VerticalVelocity, vel.y);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Item"))
            {
                var item = other.GetComponent<CollectibleItem>();
                if (item != null)
                {
                    item.GetItem();
                }
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Enemy"))
            {
                var enemy = other.gameObject.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    var topHit = enemy.Hit(other.contacts);

                    if (topHit)
                    {
                        rigidBody.AddForce(new Vector2(0, jumpSpeed), ForceMode2D.Impulse);
                    }
                    else
                    {
                        Die();
                    }
                }
            }
        }

        private void OnCollisionStay2D(Collision2D other)
        {
            if (timeSinceJump > 0.2f && rigidBody.velocity.y <= 0 && !grounded && other.gameObject.CompareTag("Ground"))
            {
                foreach (var contact in other.contacts)
                {
                    if (Vector2.Dot(contact.normal, Vector2.up) > upThreshold)
                    {
                        grounded = true;
                        canJump = true;
                        timeSinceJump = 0;
                        break;
                    }
                }
            }
        }

        private void OnCollisionExit2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Ground"))
            {
                grounded = false;
            }
        }

        private void Die()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}