﻿using AlumnoEjemplos.Los_Borbotones;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using TgcViewer;
using TgcViewer.Utils._2D;
using TgcViewer.Utils.Input;
using TgcViewer.Utils.Particles;
using TgcViewer.Utils.Sound;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;

namespace AlumnoEjemplos.Los_Borbotones
{
    public class Player1:GameObject
    {
        public Weapon weapon;
        public Sniper sniper;
        public RocketLauncher launcher;

        TgcStaticSound hitSound;
        TgcStaticSound breathSound;
        TgcStaticSound walkSound;
        TgcStaticSound runSound;
        bool running;
        string breathingSoundDir = GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Audio/Player/Breathing.wav";
        string hitSoundDir = GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Audio/Player/Hit.wav";
        string walkSoundDir = GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Audio/Player/Walk.wav";
        string runSoundDir = GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Audio/Player/Run.wav";
        public Vector3 prevEye;
        public int vida;
        double intensidadMaximaEscalable = Math.Pow(0.5, 2);
        float sprintTime;
        float tiredTime;
        float MAX_SPRINT_TIME = 5;
        float TIRED_TIME = 4.5f;
        
        float ZOOM_DELAY = 0;
        float MAX_ZOOM_DELAY = 0.2f;

        public TgcMesh meshAuxiliarParaSonido;

        public override void Init()
        {
            vida = 100;
            sprintTime = 0;
            tiredTime = 0;
            running = false;

            hitSound = new TgcStaticSound();
            breathSound = new TgcStaticSound();
            walkSound = new TgcStaticSound();
            runSound = new TgcStaticSound();

            sniper = new Sniper();
            sniper.Init();
            launcher = new RocketLauncher();
            launcher.Init();

            weapon = sniper;
            weapon.muzzle.scale = weapon.scaleMuzzle;

            //Mesh auxiliar para el sonido
            TgcSceneLoader loader = new TgcSceneLoader();
            TgcScene scene2 = loader.loadSceneFromFile(GuiController.Instance.AlumnoEjemplosMediaDir + "Los_Borbotones\\Meshes\\svd\\svd-TgcScene.xml");
            meshAuxiliarParaSonido = scene2.Meshes[0];

            //hago que los 3dSound sigan al arma
            GuiController.Instance.DirectSound.ListenerTracking = meshAuxiliarParaSonido;
            ///////////////CONFIGURAR CAMARA PRIMERA PERSONA CUSTOM//////////////////
            //Camara en primera persona, tipo videojuego FPS
            //Solo puede haber una camara habilitada a la vez. Al habilitar la camara FPS se deshabilita la camara rotacional
            //Por default la camara FPS viene desactivada
            GuiController.Instance.RotCamera.Enable = false;
            CustomFpsCamera.Instance.Enable = true;
            GuiController.Instance.CurrentCamera =  CustomFpsCamera.Instance;
            //Configurar posicion y hacia donde se mira
            CustomFpsCamera.Instance.setCamera(new Vector3(0, 930, 0), new Vector3(-400, 930, 0));

            prevEye = CustomFpsCamera.Instance.eye;
            //cargar sonido
            breathSound.loadSound(breathingSoundDir, HUDManager.Instance.PLAYER_VOLUME);
            hitSound.loadSound(hitSoundDir, HUDManager.Instance.PLAYER_VOLUME);
            //reproducir sonido de respawn
            playSound(walkSound, walkSoundDir, true);
            playSound(runSound, runSoundDir, true);
            runSound.stop();
        }

        public override void Update(float elapsedTime)
        {
           /* string weap = (string)GuiController.Instance.Modifiers.getValue("Arma");
            switch (weap)
            {
                case "Sniper":
                    weapon = sniper;
                    break;
                
                case "Rocket Launcher":
                    weapon = launcher;
                    break;
            }*/

            

            //update de la pos del mesh auxiliar
            meshAuxiliarParaSonido.Position = CustomFpsCamera.Instance.eye;
            //Procesamos input de teclado
            TgcD3dInput input = GuiController.Instance.D3dInput;
            //Seteamos las teclas
            if (GuiController.Instance.D3dInput.buttonDown(TgcD3dInput.MouseButtons.BUTTON_LEFT) && weapon.FIRE_DELAY <= 0)
            {
                weapon.FIRE_DELAY = weapon.MAX_DELAY;
                weapon.muzzle.MUZZLE_DELAY = weapon.muzzle.MAX_DELAY;
                weapon.muzzle.TIME_RENDER = weapon.muzzle.MAX_RENDER;
                weapon.fireWeapon();
                CustomFpsCamera.Instance.rotateSmoothly(-0.30f, -1.5f, 0);
            }

            if (weapon.FIRE_DELAY > 0) { weapon.FIRE_DELAY -= elapsedTime; }
            if (weapon.muzzle.MUZZLE_DELAY > 0) { weapon.muzzle.MUZZLE_DELAY -= elapsedTime; }
            if (weapon.muzzle.TIME_RENDER > 0) { weapon.muzzle.TIME_RENDER -= elapsedTime; }


            if (GuiController.Instance.D3dInput.buttonPressed(TgcD3dInput.MouseButtons.BUTTON_RIGHT) && ZOOM_DELAY <= 0.5f)
            {
                ZOOM_DELAY = MAX_ZOOM_DELAY;
                HUDManager.Instance.zoomCamera();
            }

            if (GuiController.Instance.D3dInput.keyDown(Key.LeftShift) && sprintTime < MAX_SPRINT_TIME)
            {
                CustomFpsCamera.Instance.MovementSpeed = 3 * CustomFpsCamera.DEFAULT_MOVEMENT_SPEED;
                sprintTime += elapsedTime;
                if (!running)
                {
                    runSound.play(true);
                    walkSound.stop();
                    running = true;
                }
                if (sprintTime > MAX_SPRINT_TIME) 
                {
                    breathSound.play(true);
                    tiredTime = 0; 
                }
            }
            else { 
                CustomFpsCamera.Instance.MovementSpeed = CustomFpsCamera.DEFAULT_MOVEMENT_SPEED;
                tiredTime += elapsedTime;
                if (running)
                {
                    runSound.stop();
                    walkSound.play(true);
                    running = false;
                }
                if (tiredTime > TIRED_TIME && sprintTime != 0) 
                {
                    breathSound.stop();
                    sprintTime = 0; 
                }
            }

            if (ZOOM_DELAY > 0) { ZOOM_DELAY -= elapsedTime; }

            //muzzleFlash.Position = WEAPON_OFFSET;

            weapon.Update(elapsedTime);

            //Maxima inclinacion sobre terreno
            float yActual;
            float yAnterior;
            float movspeed = CustomFpsCamera.Instance.MovementSpeed;
            GameManager.Instance.interpoledHeight(CustomFpsCamera.Instance.eye.X, CustomFpsCamera.Instance.eye.Z, out yActual);
            GameManager.Instance.interpoledHeight(prevEye.X, prevEye.Z, out yAnterior);
            double diferenciaPotenciada = Math.Pow((yActual - yAnterior) / (movspeed * elapsedTime), 2);

            if ( diferenciaPotenciada >= intensidadMaximaEscalable)
            {
                CustomFpsCamera.Instance.eye = prevEye;
                CustomFpsCamera.Instance.reconstructViewMatrix(false);
            }


            //Colision de la camara con sliding
            List<TgcMesh> obstaculos = new List<TgcMesh>();
            obstaculos = GameManager.Instance.quadTree.findMeshesToCollide(CustomFpsCamera.Instance.boundingBox);

            foreach (TgcMesh obstaculo in obstaculos)
            {
                Vector3 dirMov = CustomFpsCamera.Instance.eye - prevEye;

                TgcCollisionUtils.BoxBoxResult result = TgcCollisionUtils.classifyBoxBox(CustomFpsCamera.Instance.boundingBox, obstaculo.BoundingBox);
                if (result == TgcCollisionUtils.BoxBoxResult.Adentro || result == TgcCollisionUtils.BoxBoxResult.Atravesando)
                {
                    CustomFpsCamera.Instance.eye = prevEye + new Vector3(dirMov.X, 0, 0);
                    CustomFpsCamera.Instance.reconstructViewMatrix(false);
                    TgcCollisionUtils.BoxBoxResult resultX = TgcCollisionUtils.classifyBoxBox(CustomFpsCamera.Instance.boundingBox, obstaculo.BoundingBox);
                    if (resultX == TgcCollisionUtils.BoxBoxResult.Adentro || resultX == TgcCollisionUtils.BoxBoxResult.Atravesando)
                    {
                        CustomFpsCamera.Instance.eye = prevEye + new Vector3(0, 0, dirMov.Z);
                        CustomFpsCamera.Instance.reconstructViewMatrix(false);

                        TgcCollisionUtils.BoxBoxResult resultZ = TgcCollisionUtils.classifyBoxBox(CustomFpsCamera.Instance.boundingBox, obstaculo.BoundingBox);
                        if (resultZ == TgcCollisionUtils.BoxBoxResult.Adentro || resultZ == TgcCollisionUtils.BoxBoxResult.Atravesando)
                        {
                            CustomFpsCamera.Instance.eye = prevEye;
                        }
                    }
                    break;
                }
            }

            if (prevEye == CustomFpsCamera.Instance.eye)
            {
                walkSound.stop();
                runSound.stop();
            }
            else if(!running) { walkSound.play(true); }

            prevEye = CustomFpsCamera.Instance.eye;
        }

        public override void Render(float elapsedTime)
        {
            weapon.Render(elapsedTime);
           // GuiController.Instance.D3dDevice.Transform.World = Matrix.Identity;
        }

        public override void dispose()
        {
            meshAuxiliarParaSonido.dispose();
            walkSound.dispose();
            runSound.dispose();
            breathSound.dispose();
            hitSound.dispose();
        }

        public void recibirAtaque(int damage)
        {
            
            hitSound.play(false);
            weapon.FIRE_DELAY = 0.5f;
            vida -= damage;
            HUDManager.Instance.refreshHealth();
            if(vida <= 0)
            {
                GameManager.Instance.gameOver();
            }
        }

        public void playSound(TgcStaticSound sound, string dir, bool loop)
        {
            //reproducir un sonido
            sound.dispose();
            sound.loadSound(dir, HUDManager.Instance.PLAYER_VOLUME);
            sound.play(loop);
        }
    }
}
