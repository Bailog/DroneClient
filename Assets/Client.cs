﻿using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Client : MonoBehaviour
{
    //сетевые переменные
    int port = 8999;
    string ip;
    bool connected = false;
    
    //переменные для обработки инпутов
    private float f, r, u, s;

    public Button ButtonConnect;
    public Button ButtonDisconnect;
    public Button ButtonExit;
    public Text MessageLog;
    public InputField InputServerIP;

    //создание нового экземпляра, который будет передавать
    //значения виртуальной оси серверу
    InputContainer inputs = new InputContainer();
    short messageID = 1000;
    NetworkClient client;

    //создание клиента и привязка функций ко всем 3 кнопкам
    void Start()
    {
        CreateClient();
        ButtonConnect.onClick.AddListener(ConnectClick);
        ButtonDisconnect.onClick.AddListener(DisconnectClick);
        ButtonDisconnect.enabled = false;
        ButtonExit.onClick.AddListener(ExitClick);
    }

    //=====================================================
    //Методы клиента
    //=====================================================

    void CreateClient()
    {
        var config = new ConnectionConfig();

        // конфигурация используемых каналов
        config.AddChannel(QosType.ReliableFragmented);
        config.AddChannel(QosType.UnreliableFragmented);

        // создать клиента и присоединить конфигурацию
        client = new NetworkClient();
        client.Configure(config, 1);
        RegisterHandlers();
    }

    void RegisterHandlers()
    {
        client.RegisterHandler(messageID, OnMessageReceived);
        client.RegisterHandler(MsgType.Connect, OnConnected);
        client.RegisterHandler(MsgType.Disconnect, OnDisconnected);
    }

    //вызывается при присоединении на сервер
    //создает новый экзмепляр и отправляет сообщение на сервер
    void OnConnected(NetworkMessage message)
    {
        connected = true;
        MessageLog.text = "Connection succesfull";
        ButtonConnect.interactable = false;
        ButtonDisconnect.interactable = true;
        MyNetworkMessage messageContainer = new MyNetworkMessage();        
        messageContainer.message = "Hello server!";        
        client.Send(messageID, messageContainer);
    }

    //вызывается при разрыве соединения
    void OnDisconnected(NetworkMessage message)
    {
        connected = false;
        ButtonDisconnect.interactable = false;
        ButtonConnect.interactable = true;
        MessageLog.text = "Lost connection from " + ip;
    }

    //вызывается при получении сообщения от сервера
    void OnMessageReceived(NetworkMessage netMessage)
    {
        var objectMessage = netMessage.ReadMessage<MyNetworkMessage>();
        Debug.Log("Message received: " + objectMessage.message);
    }

    //обработка инпутов и отправка на сервер
    private void FixedUpdate()
    {
        if (connected)
        {
            inputs.f = Input.GetAxis("Horizontal");
            inputs.r = Input.GetAxis("Vertical");
            inputs.u = Input.GetAxis("Trigger");
            inputs.s = Input.GetAxis("Horizontal2");

            inputs.reset = Input.GetButton("Jump");
            /*
            if (Input.GetButtonUp("Jump"))
            {
                MyNetworkMessage messageContainer = new MyNetworkMessage();
                messageContainer.message = "Reset";
                client.Send(messageID, messageContainer);
            }
            */
            client.Send(messageID, inputs);
        }
    }

    //=====================================================
    //Кнопки
    //=====================================================
    //подключение по введеному в поле ip адресу и вывод сообщения на 
    //экран с результатом подлкючения
    public void ConnectClick()
    {
        ip = InputServerIP.text;
        MessageLog.text = "Connecting to " + ip;
        client.Connect(ip, port);
    }
    
    //отключение от сервера и вывод на экран сообщения
    public void DisconnectClick()
    {
        client.Disconnect();
        MessageLog.text = "Disconnected from " + ip;
        ButtonDisconnect.interactable = false;
        ButtonConnect.interactable = true;
    }
    
    //выход из приложения, если клиент подключен, то
    //сначала вызывается Disconnect()
    public void ExitClick()
    {
        if (connected) client.Disconnect();
        Application.Quit();
    }
}
