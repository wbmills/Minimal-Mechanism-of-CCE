using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class killonhit : MonoBehaviour
{
    public Canvas c;
    public TMP_Text score;
    public GameObject end;
    public TMP_Text finalScore;
    
    public void kill()
    {
        c.gameObject.SetActive(true);
        finalScore.text = score.text;
        score.transform.parent.gameObject.SetActive(false);
        //t.text = "You Died!\nPress Esc at any time to go back to Waiting Room";
        StartCoroutine("WaitAndRespawn");
    }

    private IEnumerator WaitAndRespawn()
    {
        end.SendMessage("SetStop");
        yield return new WaitForSeconds(5);
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
}
