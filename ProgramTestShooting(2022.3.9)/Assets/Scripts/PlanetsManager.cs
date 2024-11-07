using UnityEngine;
using System.Collections;

public class PlanetsManager : MonoBehaviour
{
    [Header("Planet Settings")]
    [SerializeField] private Sprite[] planetSprites;
    [SerializeField] private float minSize = 0.5f;
    [SerializeField] private float maxSize = 3f;
    [SerializeField] private float baseSpeed = 5f;
    [SerializeField] private float spawnHeightRange = 4f; 

    private GameManager gameManager;
    public float offset = 2f; 

    private void Start()
    {
        gameManager = GameManager.instance;
        SpawnPlanet();
    }

    public void SpawnPlanet()
    {
        if (planetSprites.Length == 0 || transform.childCount == 0) return;

        var planet = transform.GetChild(0);

        SpriteRenderer spriteRenderer = planet.GetComponent<SpriteRenderer>();
        if (spriteRenderer)
        {
            spriteRenderer.sprite = planetSprites[Random.Range(0, planetSprites.Length)];
        }

        float size = Random.Range(minSize, maxSize);
        planet.transform.localScale = Vector3.one * size;

        float planetWidth = spriteRenderer.bounds.size.x;

        float spawnX = gameManager.rightScreenBound + planetWidth / 2 + offset;
        float spawnY = Random.Range(-spawnHeightRange, spawnHeightRange);
        planet.position = new Vector3(spawnX, spawnY, 0);

        float speed = baseSpeed /Mathf.Sqrt(size);

        StartCoroutine(MovePlanet(planet, speed, gameManager.leftScreenBound - planetWidth / 2 - offset, spriteRenderer));
    }

    private IEnumerator MovePlanet(Transform planet, float speed, float leftEdge, SpriteRenderer spriteRenderer)
    {
        while (planet.position.x > leftEdge)
        {
            planet.Translate(Vector3.left * speed * Time.deltaTime);
            yield return null;
        }

        spriteRenderer.sprite = null;

        SpawnPlanet();
    }
}
