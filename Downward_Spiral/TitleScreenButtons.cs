using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class TitleScreenButtons : MonoBehaviour
{

    public GameObject optionImage;
    public GameObject creditImage;

    [SerializeField]
    float transtionTime = 1f;
    [SerializeField]
    string transtionAnimationName = "Window";
    [SerializeField]
    Animator transitionAnimator = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator StartLoadingGame() {
        Internals.transitionName = transtionAnimationName;
        transitionAnimator?.SetTrigger(transtionAnimationName + "_Start");
        yield return new WaitForSeconds(transtionTime);
        SceneManager.LoadScene("SampleScene");
    }

    public void BtnStartClicked() {
        StartCoroutine(StartLoadingGame());
        
    }
    public void BtnOptionClicked()
    {
        optionImage.SetActive(!optionImage.activeSelf);

    }
    public void BtnCreditClicked()
    {
        creditImage.SetActive(!creditImage.activeSelf);
    }
    public void BtnExitClicked()
    {
        Application.Quit();
    }
}
