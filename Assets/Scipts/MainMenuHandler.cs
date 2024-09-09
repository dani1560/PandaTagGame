using Firebase.Analytics;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class MainMenuHandler : MonoBehaviour
{
    public static MainMenuHandler Instance;
    public GameData gameData;

    public GameObject camObj;
    public GameObject[] positions;

    [Header("Panels")]
    public GameObject environmentSelectionPanel;
    public GameObject hunterVsRunnerSelectionPanel;
    public GameObject[] UImenus;

    [Header("Customization")]
    public SkinChanger changer;
    public bool isHatChange;
    public GameObject hatSelectedBtn;
    public GameObject skinSelectedBtn;
    public int currentHat;
    public int currentMat;
    public TextMeshProUGUI selectBtnTxt;
    public GameObject selectBtnAdLogo;
    public GameObject[] HighlightCustomize;
    public GameObject mainMenuPanda;
    public GameObject customizationPanda;
    public Sprite Unlock, Select, Selected;
    public Image SelectBTNimg;
    [Header("Mode")]
    public GameObject runnerVsHunterBtn;
    public GameObject bombTagBtn;
    public GameObject[] ModeSelectGlow;
    public GameObject StartGameBTN;

    [Header("Mode Runner Vs Hunter")]
    public GameObject runnerBtn;
    public GameObject hunterBtn;
    public GameObject[] HunterRunnerGlowBTNS;
    [Header("Environment")]
    public GameObject[] EnvironmentPanels;
    public int environmentSelected;
    public GameObject[] EnvSelectionGlowBTN;

    private void Awake()
    {
        Instance = this;
        Time.timeScale = 1.0f;
    }

    private void Start()
    {
        SelectCustomizationType(true);
        environmentSelected = 1;
        SelectMode(GameManager.Instance.isBombTag);
        SelectRunnerMode(GameManager.Instance.isRunner);

        if(PlayerPrefs.GetInt("PopUpCount")!=1 ||!PlayerPrefs.HasKey("PopUpCount"))
        {
            PlayerPrefs.SetInt("PopUpCount", PlayerPrefs.GetInt("PopUpCount") + 1);
            Debug.Log("Rate us Pop Show Progress :" + PlayerPrefs.GetInt("PopUpCount") + "/2");
        }
        else if(PlayerPrefs.GetInt("PopUpCount")==1)
        {
            RateGame.Instance.ForceShowRatePopupWithCallback(PopupClosedMethod);
        }
    }
    private void PopupClosedMethod(GleyRateGame.PopupOptions result)
    {
        Debug.Log("Popup Closed-> ButtonPresed: " + result + " -> Resume Game");
    }

    #region Settings
    public void Quit()
    {
        Application.Quit();
    }
    #endregion

    #region Customization
    public void OpenCustomizationPanel()
    {
        changer.SetHat(GameManager.Instance.hatSelected);
        changer.SetMaterial(GameManager.Instance.skinSelected);
        currentHat = GameManager.Instance.hatSelected;
        currentMat = GameManager.Instance.skinSelected;
        SetIsLocked(false, true); // Assuming it is unlocked
    }

    public void SetIsLocked(bool isLocked, bool isUnlocked)
    {
        selectBtnAdLogo.SetActive(isLocked && !isUnlocked);
        if (isLocked && !isUnlocked)
        {
            selectBtnTxt.text = "Lock";
            SelectBTNimg.sprite = Unlock;
        }
        else if (isUnlocked)
        {
            selectBtnTxt.text = "Select";
            SelectBTNimg.sprite = Select;
        }
        else
        {
            
            selectBtnTxt.text = "Selected";
            SelectBTNimg.sprite = Selected;
        }
    }

    public void SelectCustomizationType(bool isHat)
    {
        isHatChange = isHat;
        hatSelectedBtn.SetActive(isHat);
        skinSelectedBtn.SetActive(!isHat);
        if (isHat)
        {
            if (gameData.hats[currentHat])
            {
                SetIsLocked(false, true);

            }
            else
            {
                SetIsLocked(true, false);
            }
        }
        else
        {
            if (gameData.skin[currentMat])
            {
                SetIsLocked(false, true);
            }
            else
            {
                SetIsLocked(true, false);
            }
        }


    }

    public void NextCustomization()
    {
        if (isHatChange)
        {
            currentHat++;
            if (currentHat >= changer.GetHatsLength()) currentHat = 0;
            
            changer.SetHat(currentHat);
            if (gameData.hats[currentHat])
            {
                SetIsLocked(false, true);
            }
            else
            {
                SetIsLocked(true, false);
            }
        }
        else
        {
            currentMat++;
            if (currentMat >= changer.GetMaterialsLength()) currentMat = 0;
            
            changer.SetMaterial(currentMat);
            if (gameData.skin[currentMat])
            {
                SetIsLocked(false, true);
            }
            else
            {
                SetIsLocked(true, false);
            }
        }
    }

    public void PrevCustomization()
    {
        if (isHatChange)
        {
            currentHat--;
            if (currentHat < 0) currentHat = changer.GetHatsLength() - 1;
            changer.SetHat(currentHat);
            if (gameData.hats[currentHat])
            {
                SetIsLocked(false, true);
            }
            else
            {
                SetIsLocked(true, false);
            }
        }
        else
        {
            currentMat--;
            if (currentMat < 0) currentMat = changer.GetMaterialsLength() - 1;
            changer.SetMaterial(currentMat);
            if (gameData.skin[currentMat])
            {
                SetIsLocked(false, true);
            }
            else
            {
                SetIsLocked(true, false);
            }
        }
    }

    public void SelectCustomization()
    {
        
        if (isHatChange)
        {
            if (!gameData.hats[currentHat])
            {
                AdmobAds.instance.ShowRewardedAd();
            }
        }
        else
        {
            if (!gameData.skin[currentMat])
            {
                AdmobAds.instance.ShowRewardedAd();
            }
        }
        PlayerPrefs.SetInt("SelectedMat", currentMat);
        PlayerPrefs.SetInt("SelectedHat", currentHat);

        SetIsLocked(false, false);

        // Log Mat and Hat
        FirebaseAnalytics.LogEvent("selected_mat", new Parameter("mat", currentMat));
        FirebaseAnalytics.LogEvent("selected_hat", new Parameter("hat", currentHat));
    }

    public void SelectedCustomization()
    {
        if (isHatChange)
        {
            GameManager.Instance.hatSelected = currentHat;
            gameData.hats[currentHat] = true;
        }
        else
        {
            gameData.skin[currentMat] = true;
            GameManager.Instance.skinSelected = currentMat;
        }

        // Call SetIsLocked to update the button text to "Selected"
        

        PersistentDataManager.instance.SaveData();

        // Apply customization to both pandas
        ApplyCustomizationToBothPandas();
    }


    private void ApplyCustomizationToBothPandas()
    {
        mainMenuPanda.GetComponent<SkinChanger>().ApplyCustomization();
        customizationPanda.GetComponent<SkinChanger>().ApplyCustomization();
    }
    #endregion

    #region Mode
    public void SelectMode(bool isBomb)
    {
        GameManager.Instance.isBombTag = isBomb;
        bombTagBtn.SetActive(isBomb);
        runnerVsHunterBtn.SetActive(!isBomb);
    }

    public void ProceedMode()
    {
        if (GameManager.Instance.isBombTag)
        {
            environmentSelectionPanel.SetActive(true);
        }
        else
        {
            hunterVsRunnerSelectionPanel.SetActive(true);
        }

        // FirebaseAnalytics.LogEvent
        FirebaseAnalytics.LogEvent("mode_select", new Parameter("mode", tempMode));
    }

    int tempMode;
    public void ModeSelect(int id)
    {
        for (int i = 0; i < ModeSelectGlow.Length; i++)
        {
            ModeSelectGlow[i].SetActive(false);
        }

        tempMode = id;
        ModeSelectGlow[id].SetActive(true);
        StartGameBTN.GetComponent<Button>().interactable = true;
    }
    #endregion

    #region Mode Runner vs Hunter
    public void SelectRunnerMode(bool isRunner)
    {
        GameManager.Instance.isRunner = isRunner;
        runnerBtn.SetActive(isRunner);
        hunterBtn.SetActive(!isRunner);
    }
    int runnerOrHunter;

    public void GlowRunnerHunter(int index)
    {
        for(int i=0;i<HunterRunnerGlowBTNS.Length;i++)
        {
            HunterRunnerGlowBTNS[i].SetActive(false);
        }
        runnerOrHunter = index;
        // FirebaseAnalytics.LogEvent
        FirebaseAnalytics.LogEvent("env_select", new Parameter("env", index));
        HunterRunnerGlowBTNS[index].SetActive(true);
    }
    #endregion

    #region Environment
    public void NextEnvironment()
    {
        if (environmentSelected < 3)
        {
            foreach (GameObject g in EnvironmentPanels) g.SetActive(false);
            environmentSelected++;
            EnvironmentPanels[environmentSelected - 1].SetActive(true);
        }

    }



    public void PrevEnvironment()
    {
        if (environmentSelected > 1)
        {
            foreach (GameObject g in EnvironmentPanels) g.SetActive(false);
            environmentSelected--;
            EnvironmentPanels[environmentSelected - 1].SetActive(true);
        }
    }

    int tempEnv;
    public void SelectEnv(int env)
    {
        foreach (GameObject g in EnvironmentPanels) g.SetActive(false);
        EnvironmentPanels[env].SetActive(true);
        environmentSelected = env + 1;

        for(int i=0;i<EnvSelectionGlowBTN.Length;i++)
        {
            EnvSelectionGlowBTN[i].SetActive(false);
        }
        tempEnv = env;
        EnvSelectionGlowBTN[env].SetActive(true);
    }

    public void Play()
    {
        // FirebaseAnalytics.LogEvent
        FirebaseAnalytics.LogEvent("env_select", new Parameter("env", tempEnv));
     
        AdmobAds.instance.ShowInterstitialAd();
        SceneManager.LoadScene(environmentSelected);
    }

    public void PlayRandom()
    {
        SceneManager.LoadScene(Random.Range(1, 4));
    }

    public void BackEnvironmentPanel()
    {
        if (GameManager.Instance.isBombTag)
        {
            environmentSelectionPanel.SetActive(false);
        }
        else
        {
            hunterVsRunnerSelectionPanel.SetActive(true);
            environmentSelectionPanel.SetActive(false);
        }
    }
    #endregion

    #region Camera Change
    public void StartFollowing(int no)
    {
        if (no == 0)
        {
            mainMenuPanda.GetComponent<SkinChanger>().ApplyCustomization();
        }
        StartCoroutine(FollowAndLookCoroutine(positions[no].transform, 1f, no)); // 2 seconds duration
        ControllUI(no);
    }

    public void ButtonSelectionCustomization(int index)
    {
        for (int i = 0; i < HighlightCustomize.Length; i++)
        {
            HighlightCustomize[i].SetActive(false);
        }

        if (index != 0)
        {
            HighlightCustomize[index].SetActive(true);
        }
    }

    void ControllUI(int PanelNo)
    {
        for (int i = 0; i < UImenus.Length; i++)
        {
            UImenus[i].SetActive(false);
        }
        UImenus[PanelNo].SetActive(true);
    }

    IEnumerator FollowAndLookCoroutine(Transform target, float duration, int num)
    {
        Vector3 startPosition = camObj.transform.position;
        Quaternion startRotation = camObj.transform.rotation;
        Quaternion targetRotation = Quaternion.LookRotation(target.forward);
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            // Calculate the fraction of the duration that has passed
            float fraction = elapsedTime / duration;

            // Interpolate position and rotation
            camObj.transform.position = Vector3.Lerp(startPosition, target.position, fraction);
            camObj.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, fraction);

            // Wait until next frame
            yield return null;

            // Update elapsed time
            elapsedTime += Time.deltaTime;
        }

        // Ensure final position and rotation match exactly
        camObj.transform.position = target.position;
        camObj.transform.rotation = targetRotation;
    }
    #endregion
}
