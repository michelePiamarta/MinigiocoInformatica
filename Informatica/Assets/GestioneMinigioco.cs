using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GestioneMinigioco : MonoBehaviour
{

    Button[] buttons= new Button[8];
    Sprite lampadinaAccesa;
    Sprite lampadinaSpenta;
    TMP_Text numero;
    int currentNumero = 255;
    public int punteggio = 0;
    private int tempoLimite;
    public TMP_Text tempoLimiteDisplay;
    public TMP_Text punteggioDisplay;
    private System.Timers.Timer timer;
    System.Random random = new System.Random();
    void Start()
    {
        //timer
        tempoLimite = 120;
        timer = new System.Timers.Timer(1000);
        timer.Elapsed += timerMinigioco;
        timer.AutoReset = true;
        timer.Enabled = true;
        //display punteggio, tempo massimo e numero da indovinare
        numero = this.GetComponentsInChildren<TMP_Text>()[0];
        tempoLimiteDisplay = this.GetComponentsInChildren<TMP_Text>()[1];
        punteggioDisplay = this.GetComponentsInChildren<TMP_Text>()[2];
        buttons = this.GetComponentsInChildren<Button>();
        //sprite per le lampadine
        lampadinaAccesa = Resources.Load<Sprite>("lampadinaAccesa");
        lampadinaSpenta = Resources.Load<Sprite>("lampadinaSpenta");
        //default tutte le lampadine spente
        foreach(var button in buttons){
            if(button.name != "conferma"){
                button.GetComponent<Image>().sprite = lampadinaSpenta;
                button.onClick.AddListener(()=>{
                    print(button.name);
                    if(button.GetComponent<Image>().sprite.Equals(lampadinaAccesa))
                        button.GetComponent<Image>().sprite = lampadinaSpenta;
                    else
                        button.GetComponent<Image>().sprite = lampadinaAccesa;
                });
            }
            else 
                button.onClick.AddListener(()=>{
                    conferma();
                });
        }
        numero.text = currentNumero.ToString();
    }

    public void conferma(){
        int guess = 0;
        for (int i = 0; i < buttons.Length-1; i++)
        {
            if(buttons[i].GetComponent<Image>().sprite.Equals(lampadinaAccesa))
                guess +=(int)Math.Pow(2,i);
        }
        if(guess == currentNumero)
            punteggio++;
        else if(punteggio > 0)
            punteggio--;
        punteggioDisplay.text = "" + punteggio;
        nuovoLivello();
    }

    public void nuovoLivello(){
        currentNumero = random.Next(256);
        numero.text = "" + currentNumero;
        foreach (var button in buttons)
        {
            if(button.name != "conferma")
                button.GetComponent<Image>().sprite = lampadinaSpenta;
        }
    }

    public void timerMinigioco(System.Object source, ElapsedEventArgs e){
        if(tempoLimite > 0){
            tempoLimiteDisplay.text = ""+ --tempoLimite;
            Debug.Log("ce ancora tempo");
        }
        else{
            Debug.Log("minigioco finito");
            timer.Stop();
            //chiamo la funzione asincrona in questo modo così posso aspettare che finisca prima di andare avanti 
            //senza far diventare questa funzione a sua volta asincrona
            var t = Task.Run(() => PostaPunteggio());
            t.Wait();
            Console.WriteLine(t.Result);
        }
    }

    public async Task<string> PostaPunteggio(){
    var response = string.Empty;
    using (var client = new HttpClient()){
        HttpResponseMessage result = await client.PostAsync("http://localhost:3000/add", new StringContent("{\"punteggio\": "+punteggio+"}", Encoding.UTF8, "application/json"));
        if (result.IsSuccessStatusCode){
            response = result.StatusCode.ToString();
        }
    }
        return response;
    }
}
