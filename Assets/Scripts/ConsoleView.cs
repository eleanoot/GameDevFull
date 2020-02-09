
// Display the UI for the developer console. 
// Adapted from code by Eliot Lash, 2014-2015.
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Collections;

public class ConsoleView : MonoBehaviour {
	ConsoleController console = new ConsoleController();
	
	bool didShow = false;

    //Container for console view.
    public GameObject viewContainer; 
	public Text logTextArea;
	public InputField inputField;

	void Start() {
		if (console != null) {
			console.visibilityChanged += onVisibilityChanged;
			console.logChanged += onLogChanged;
		}
		updateLogStr(console.log);
	}
	
	~ConsoleView() {
		console.visibilityChanged -= onVisibilityChanged;
		console.logChanged -= onLogChanged;
	}
	
	void Update() {
        // Toggle visibility when equals key pressed and pause the game to enter commands.
        if (Input.GetKeyUp(KeyCode.Equals))
        {
            if (Time.timeScale == 1)
            {
                Time.timeScale = 0;
            }
            else if (Time.timeScale == 0)
            {
                Time.timeScale = 1;
            }
            toggleVisibility();
        }
	}

	void toggleVisibility() {
		setVisibility(!viewContainer.activeSelf);
	}
	
	void setVisibility(bool visible) {
		viewContainer.SetActive(visible);
	}
	
	void onVisibilityChanged(bool visible) {
		setVisibility(visible);
	}
	
	void onLogChanged(string[] newLog) {
		updateLogStr(newLog);
	}
	
	void updateLogStr(string[] newLog) {
		if (newLog == null) {
			logTextArea.text = "";
		} else {
			logTextArea.text = string.Join("\n", newLog);
		}
	}

	
	// Event that should be called by anything wanting to submit the current input to the console.
	public void runCommand() {
		console.runCommandString(inputField.text);
		inputField.text = "";
	}

}
