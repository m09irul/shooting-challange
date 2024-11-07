using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Stage main loop
/// </summary>
public class StageLoop : MonoBehaviour
{
	#region static 
	static public StageLoop Instance { get; private set; }
	#endregion

	//
	public TitleLoop m_title_loop;

	[Header("Layout")]
	public Transform m_stage_transform;
	public TextMeshProUGUI m_stage_score_text;

	[Header("Prefab")]
	public Player m_prefab_player;
	public EnemySpawner m_prefab_enemy_spawner;

    //
    public int m_game_score { get; private set; } = 1;


	//------------------------------------------------------------------------------
	
	#region loop
	public void StartStageLoop()
	{

		StartCoroutine(StageCoroutine());
        TransitionManager.instance.FadeOut(5f,
            () =>
			{
                CreatePlayer();
            });

        
	}

	/// <summary>
	/// stage loop
	/// </summary>
	private IEnumerator StageCoroutine()
	{
		Debug.Log("Start StageCoroutine");

		SetupStage();

		while (true)
		{
			if (Input.GetKeyDown(KeyCode.Space) && GameManager.instance.isGameOver)
			{
				TransitionManager.instance.FadeIn(2, 
					()=>
					{
						SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
					});
			}
			yield return null;
		}
	}
	#endregion


	void SetupStage()
    {
        Instance = this;

        m_game_score = 0;
        RefreshScore();
    }

    private void CreatePlayer()
    {
        //create player

        Player player = Instantiate(m_prefab_player, m_stage_transform);
        if (player)
        {
            player.transform.position = new Vector3(GameManager.instance.leftScreenBound + 3, GameManager.instance.topScreenBound + 3, 0);

            LeanTween.moveY(player.gameObject, 0, 5f).setEase(LeanTweenType.easeOutCirc)
                .setOnComplete(() =>
                {
					CameraShakeManager.instance.ShakeCamera(9, 0.5f);
                    player.StartRunning();
                    WaveManager.instance.StartGame();

                });
        }
    }

    void CleanupStage()
	{
		//delete all object in Stage
		{
			for (var n = 0; n < m_stage_transform.childCount; ++n)
			{
				Transform temp = m_stage_transform.GetChild(n);
				Destroy(temp.gameObject);
			}
		}

		Instance = null;
	}

	//------------------------------------------------------------------------------

	public void AddScore(int a_value)
	{
		m_game_score += a_value;
		RefreshScore();
	}

	void RefreshScore()
	{
		if (m_stage_score_text)
		{
			m_stage_score_text.text = $"Stage {m_game_score} of 6";
		}
	}
}
