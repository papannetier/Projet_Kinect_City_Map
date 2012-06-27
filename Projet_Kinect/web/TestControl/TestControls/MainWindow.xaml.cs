using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using System.Timers;
using Kinect.Toolbox;
using Kinect.Toolbox.Record;
using Kinect.Toolbox.Gestures;
using KinectControls;
using Coding4Fun.Kinect;
using Coding4Fun.Kinect.Wpf;
using Microsoft.Speech.Recognition;
using Microsoft.Speech.AudioFormat;
using System.IO;

namespace TestControls
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        KinectSensor kinectSensor;
        NotreGestureDetecteur swipeGestureReconnaissance;
        readonly ColorStreamManager colorManager = new ColorStreamManager();
        readonly DepthStreamManager depthmanager = new DepthStreamManager();
        SkeletonDisplayManager skeletonDisplayManager;
        readonly BarycenterHelper barycenterHelper = new BarycenterHelper();
        public System.Timers.Timer aTimer = new System.Timers.Timer();
        public Boolean timerfini;
        public Boolean swipebacktofront=false;
        public Boolean swipefronttoback=false;
        public Boolean swipeup = false;
        public Boolean swipebottom = false;
        
        bool displayDepth;
        SkeletonReplay replay;
        BindableNUICamera nuiCamera;
        private Skeleton[] skeletons;
       // private Boolean gestureAutorise = false;
       // public HoverButton btn=null;
        public HoverButton zone = null;
        private List<HoverButton> zoneDeposable = new List<HoverButton>();
        private ZoneDeDepot lesZonesDepots = null;
        private NomVille villes = new NomVille();
        //private String villeprincipale = null;
        //audi
        private SpeechRecognitionEngine speechEngine;

        //autre gesture
        Boolean mainGauche = false;
        Boolean tete = false;
        private Joint jointMainGauche;
        private Joint jointTete;

        public MainWindow()
        {
            InitializeComponent();
        }
        


        void Kinects_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            switch(e.Status)
            {
                case KinectStatus.Connected :
                    if(kinectSensor == null)
                    {
                        kinectSensor = e.Sensor;
                        Initialize();
                    }
                    break;

                case KinectStatus.Disconnected :
                    if(kinectSensor == e.Sensor)
                    {
                        Clean();
                        MessageBox.Show("La kinect a été déconnectée");
                    }
                    break;

                case KinectStatus.NotReady :
                    break;

                case KinectStatus.NotPowered :
                    if(kinectSensor == e.Sensor)
                    {
                        Clean();
                        MessageBox.Show("La kinect n'est plus allumée");
                    }
                    break;

                default : 
                    MessageBox.Show("Unhandled Status : " +e.Status);
                    break;
            }
        }





        private void HoverButton_Click(object sender, EventArgs e)
        {
           
            btn1.Check(hand);
        }
        



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                KinectSensor.KinectSensors.StatusChanged += Kinects_StatusChanged;


                foreach(KinectSensor kinect in KinectSensor.KinectSensors)
                {
                    if(kinect.Status == KinectStatus.Connected)
                    {
                        kinectSensor = kinect;
                        break;
                    }

                }

                if(KinectSensor.KinectSensors.Count == 0)
                {
                    MessageBox.Show("Pas de Kinect trouvée");

                }
                else 
                    Initialize();
            }

            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }



        private void Initialize()
        {
            InitialisationVille();
            if(kinectSensor == null)
                return;

            //kinectSensor.ColorStream.Enable(ColorImageFormat.RgbResolution1280x960Fps12);
            kinectSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

            kinectSensor.ColorFrameReady += kinectRuntime_ColorFrameReady;

            kinectSensor.DepthStream.Enable(DepthImageFormat.Resolution80x60Fps30);
            kinectSensor.DepthFrameReady += kinectSensor_DepthFrameReady;

            kinectSensor.SkeletonStream.Enable(new TransformSmoothParameters
            {
                Smoothing = 0.5f,
                Correction = 0.5f,
                Prediction = 0.5f,
                JitterRadius = 0.05f,
                MaxDeviationRadius = 0.04f
            });

            kinectSensor.SkeletonFrameReady += kinectRuntime_SkeletonFrameReady;

            swipeGestureReconnaissance = new NotreGestureDetecteur();
            swipeGestureReconnaissance.OnGestureDetected += OnGestureDetected;

            skeletonDisplayManager = new SkeletonDisplayManager(kinectSensor, kinectCanvas);
            kinectSensor.Start();

            nuiCamera = new BindableNUICamera(kinectSensor);
            zoneDeposable.Add(zoneLille);
            zoneDeposable.Add(zoneAix);
            zoneDeposable.Add(zoneBordeaux);
            zoneDeposable.Add(zoneLyon);
            zoneDeposable.Add(zoneNancy);
            zoneDeposable.Add(zoneNantes);
            zoneDeposable.Add(zoneOrléans);
            zoneDeposable.Add(zoneRennes);
            zoneDeposable.Add(zoneToulouse);
            zoneDeposable.Add(zoneParis);
            lesZonesDepots = new ZoneDeDepot(zoneDeposable);

            RecognizerInfo ri = GetKinectRecognizer();
            if (null != ri)
            {       

               this.speechEngine = new SpeechRecognitionEngine(ri.Id);

               try
               {
                   // Create a grammar from grammar definition XML file.
                   using (var memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(Properties.Resources.SpeechGrammar)))
                   {
                       Grammar g = new Grammar(memoryStream);
                       speechEngine.LoadGrammar(g);
                       Console.WriteLine("Reconnaissance vocale");
                   }
               }
               catch (ArgumentNullException a)
               {
                   Console.WriteLine("ERRREUR RECONNAISSANCE "+a);
               }
               catch (InvalidOperationException e)
               {
                    Console.WriteLine("ERRREUR RECONNAISSANCE "+e);
               }
               speechEngine.SpeechRecognized += SpeechRecognized;
                
                speechEngine.SetInputToAudioStream(
                    kinectSensor.AudioSource.Start(), new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
                speechEngine.RecognizeAsync(RecognizeMode.Multiple);
            }
           //Initialisation des boutons des villes
            String villeprincipale = villes.getNomVilleInitiale();
            btn1.Text = villeprincipale;
            textBoxInfoVille.FontSize = 18;
            LogTextBox.FontSize = 18;
            suivant.Text = villes.recupererNomVille(villeprincipale, "previous");
            precedent.Text = villes.recupererNomVille(villeprincipale, "next");
            textBoxInfoVille.Text = InformationVille.informationLille(); //lille debut
            Lille1Logo.Opacity = 1;//lille debut
            Lille1Logo.LoadedBehavior = MediaState.Play; //lilleDebut
            //a voir
            this.Cursor = Cursors.None;
        }
                



        void kinectSensor_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            if(!displayDepth)
                return;

            using(var frame = e.OpenDepthImageFrame())
            {
                if(frame == null)
                    return;

                depthmanager.Update(frame);
            }
        }
           

        void kinectRuntime_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            if(!displayDepth)
                return;

            using(var frame = e.OpenColorImageFrame())
            {
                if(frame == null)
                    return;

                colorManager.Update(frame);
            }
        }


        void kinectRuntime_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if(frame == null)
                    return;


                Tools.GetSkeletons(frame, ref skeletons);

                if(skeletons.All(s => s.TrackingState == SkeletonTrackingState.NotTracked))
                    return;

                ProcessFrame(frame);

            }
        }

        

        void ProcessFrame(ReplaySkeletonFrame frame)
        {
            foreach(var skeleton in frame.Skeletons)
            {
                if(skeleton.TrackingState != SkeletonTrackingState.Tracked)
                    continue;

                barycenterHelper.Add(skeleton.Position.ToVector3(), skeleton.TrackingId);

                if(!barycenterHelper.IsStable(skeleton.TrackingId))
                    return;


                foreach(Joint joint in skeleton.Joints)
                {
                    if(joint.TrackingState != JointTrackingState.Tracked)
                        continue;

                    int positionInit = villes.getPositionElement("paris");



                    

                     if (joint.JointType == JointType.HandRight)
                     {
                        sonDebutDeJeu.LoadedBehavior = MediaState.Play; 
                        int positionVilleDansTableau = villes.getPositionElement();
                        swipeGestureReconnaissance.Add(joint.Position, kinectSensor);
                        hand.SetPosition(joint);

                         

                        if (btn1.Check(hand))
                        {
                            Console.WriteLine("la main est dans la zone de droite et sur le bouton principal");

                            if (swipefronttoback)
                            {
                                btn1.BackgroundColor = Brushes.Chocolate;
                                btn1.SetPosition(joint);



                            }

                        }

                        if (swipebacktofront)
                        {                           
                           // zone.BackgroundColor = Brushes.CadetBlue;
                            zone.Opacity = 1;
                            zone.Text = btn1.Text;
                            zone.TextColor = Brushes.Black;
                            BoutonPositionInitial();
                                                   
                            lesZonesDepots.verifierDoublonsOnMap(zone);
                            if (lesZonesDepots.reponse().Equals("Maintenant que les villes sont placées. Vous pouvez demander la correction"))
                            {
                                sonToutesLesVillesPlacees.Position = new TimeSpan(0);
                                sonToutesLesVillesPlacees.LoadedBehavior = MediaState.Play;
                            }
                            swipefronttoback = false;
                            swipebacktofront = false;
                        }

                        if (ZonePoubelle.Check(hand))
                        {
                            BoutonPositionInitial();
                            swipefronttoback = false;
                          
                        }
                        if (zoneDefilementBoutonVille.Check(hand))//si la main est dans la zone de droite avec les boutons
                        {

                            if (swipebottom)
                            {
                                sonVent.Position = new TimeSpan(0);
                                sonVent.LoadedBehavior = MediaState.Play;
                                

                                suivant.Text = btn1.Text;
                                btn1.Text = precedent.Text;
                                precedent.Text = villes.recupererNomVille(btn1.Text, "next");
                                informationMiageVille();
                                Console.WriteLine("main droite defilement vers le bas");


                                swipebottom = false;
                            }

                                                                                  
                        }

                    }

                     if (joint.JointType == JointType.HandLeft)
                     {
                         mainGauche = true;
                         jointMainGauche = joint;
                         Console.WriteLine("main gauche trouvée");

                     }
                     if (joint.JointType == JointType.Head)
                     {
                         tete = true;
                         jointTete = joint;
                         Console.WriteLine("tete trouvée");
                     }
                     Console.WriteLine("coucou");
                     zone = GetZone();
                     if (tete && mainGauche && zone!= null && zone.Check(hand) && btn1.Check(hand))
                     {
                         Console.WriteLine("Main + tete trouvée");
                         if (jointMainGauche.Position.Y > jointTete.Position.Y)
                         {
                             BoutonPositionInitial();
                             Console.WriteLine("Main gauche au dessus de tete");


                             //zone.BackgroundColor = Brushes.CadetBlue;
                             zone.Opacity = 1;
                             zone.Text = btn1.Text;
                             swipefronttoback = false;
                             zone.TextColor = Brushes.Black;
                             
                            lesZonesDepots.verifierDoublonsOnMap(zone);
                            if (lesZonesDepots.reponse().Equals("Maintenant que les villes sont placées. Vous pouvez demander la correction"))
                            {
                                sonToutesLesVillesPlacees.Position = new TimeSpan(0);
                                sonToutesLesVillesPlacees.LoadedBehavior = MediaState.Play;
                            }
                         }
                         tete = false;
                         mainGauche = false;
                    }

                }

         }
        }
        private void BoutonPositionInitial()
        {
            Canvas.SetLeft(btn1, 1086);
            Canvas.SetTop(btn1, 336);
            btn1.BackgroundColor = Brushes.Blue;
        }

         void OnGestureDetected(string gesture)
        {
            if (gesture.Equals("SwipeUp"))
            {
                swipeup = true;
                Console.WriteLine("swipe defilement vers le haut");
            }


            if (gesture.Equals("SwipeBottom"))
            {
                swipebottom = true;
                Console.WriteLine("swipe defilement vers le bas");
            }
          
                
                if (gesture.Equals("SwipeBackFront"))
                {
                     if (GetZone()!=null)//Vérifie si le bouton peut etre deposable sur la zone
                    {
                        zone = GetZone();
                        swipebacktofront = true;
                        Console.WriteLine("swipe back front");
                    }
                    
                }

                if (gesture.Equals("SwipeFrontBack"))
                {
                    if (btn1.Check(hand))
                    {
                        swipefronttoback = true;
                    }
                    Console.WriteLine("swipe front back");
                }
             

                
            }      
        

        HoverButton GetButton()
         {
             if (btn1.Check(hand)) return btn1;
             
             else return null;

         }
        
         HoverButton GetZone()
         {
             if (zoneParis.Check(hand)) return zoneParis;
             else if (zoneLille.Check(hand)) return zoneLille;
             else if (zoneAix.Check(hand)) return zoneAix;
             else if (zoneBordeaux.Check(hand)) return zoneBordeaux;
             else if (zoneLyon.Check(hand)) return zoneLyon;
             else if (zoneNancy.Check(hand)) return zoneNancy;
             else if (zoneNantes.Check(hand)) return zoneNantes;
             else if (zoneOrléans.Check(hand)) return zoneOrléans;
             else if (zoneRennes.Check(hand)) return zoneRennes;
             else if (zoneToulouse.Check(hand)) return zoneToulouse;
             else return null;
         }


        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            timerfini = true;

        }
       

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Clean();
        }


        private void Clean()
        {
            if(swipeGestureReconnaissance != null)
            {
                swipeGestureReconnaissance.OnGestureDetected -= OnGestureDetected;
            }
            if (speechEngine != null)
            {
                speechEngine.SpeechRecognized -= SpeechRecognized;
                speechEngine.RecognizeAsyncStop();
            }
         
            
            if(kinectSensor != null)
            {
                kinectSensor.ColorFrameReady -= kinectRuntime_ColorFrameReady;
                kinectSensor.SkeletonFrameReady -= kinectRuntime_SkeletonFrameReady;
                kinectSensor.DepthFrameReady -= kinectSensor_DepthFrameReady;
                kinectSensor.Stop();
                kinectSensor = null;
            }
        }

        private void hand_Loaded(object sender, RoutedEventArgs e)
        {

        }


        /****reconnaissance vocale ***/

         private static RecognizerInfo GetKinectRecognizer()
        {
            foreach (RecognizerInfo recognizer in SpeechRecognitionEngine.InstalledRecognizers())
            {
                string value;
                recognizer.AdditionalInfo.TryGetValue("Kinect", out value);
                if ("True".Equals(value, StringComparison.OrdinalIgnoreCase) && "en-US".Equals(recognizer.Culture.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return recognizer;
                }
            }
            
            return null;
        }
        
        
        private void SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            // Speech utterance confidence below which we treat speech as if it hadn't been heard
            const double ConfidenceThreshold = 0.6;

            if (e.Result.Confidence >= ConfidenceThreshold)
            {
                switch (e.Result.Semantics.Value.ToString())
                {                   
                    case "ZONE1":
                        zoneLille.Text = btn1.Text;
                        zoneLille.Opacity = 1;
                        lesZonesDepots.verifierDoublonsOnMap(zoneLille);
                        zoneLille.TextColor = Brushes.Black;
                        Console.WriteLine("zone1");
                        LogTextBox.Text = "zone1";
                        LogTextBox.Text = lesZonesDepots.reponse();
                        if (lesZonesDepots.reponse().Equals("Maintenant que les villes sont placées. Vous pouvez demander la correction"))
                        {
                            sonToutesLesVillesPlacees.Position = new TimeSpan(0);
                            sonToutesLesVillesPlacees.LoadedBehavior = MediaState.Play;
                        }
                        break;
                    case "ZONE2":
                        zoneNancy.Text = btn1.Text;
                        zoneNancy.Opacity = 1;
                        lesZonesDepots.verifierDoublonsOnMap(zoneNancy);
                        zoneNancy.TextColor = Brushes.Black;
                        Console.WriteLine("zone2");
                        LogTextBox.Text = "zone2";
                        LogTextBox.Text = lesZonesDepots.reponse();
                        if (lesZonesDepots.reponse().Equals("Maintenant que les villes sont placées. Vous pouvez demander la correction"))
                        {
                            sonToutesLesVillesPlacees.Position = new TimeSpan(0);
                            sonToutesLesVillesPlacees.LoadedBehavior = MediaState.Play;
                        }
                        break;
                    case "ZONE3":
                        zoneBordeaux.Text = btn1.Text;
                        zoneBordeaux.Opacity = 1;
                        lesZonesDepots.verifierDoublonsOnMap(zoneBordeaux);
                        zoneBordeaux.TextColor = Brushes.Black;
                        Console.WriteLine("zone3");
                        LogTextBox.Text = "zone3";
                        LogTextBox.Text = lesZonesDepots.reponse();
                        if (lesZonesDepots.reponse().Equals("Maintenant que les villes sont placées. Vous pouvez demander la correction"))
                        {
                            sonToutesLesVillesPlacees.Position = new TimeSpan(0);
                            sonToutesLesVillesPlacees.LoadedBehavior = MediaState.Play;
                        }
                        break;
                    case "ZONE4":
                        zoneNantes.Text = btn1.Text;
                        zoneNantes.Opacity = 1;
                        lesZonesDepots.verifierDoublonsOnMap(zoneNantes);
                        zoneNantes.TextColor = Brushes.Black;
                        Console.WriteLine("zone4");
                        LogTextBox.Text = "zone4";
                        LogTextBox.Text = lesZonesDepots.reponse();
                        if (lesZonesDepots.reponse().Equals("Maintenant que les villes sont placées. Vous pouvez demander la correction"))
                        {
                            sonToutesLesVillesPlacees.Position = new TimeSpan(0);
                            sonToutesLesVillesPlacees.LoadedBehavior = MediaState.Play;
                        }
                        break;
                     case "ZONE5":
                        zoneRennes.Text = btn1.Text;
                        zoneRennes.Opacity = 1;
                        lesZonesDepots.verifierDoublonsOnMap(zoneRennes);
                        zoneRennes.TextColor = Brushes.Black;
                        Console.WriteLine("zone5");
                        LogTextBox.Text = "zone5";
                        LogTextBox.Text = lesZonesDepots.reponse();
                        if (lesZonesDepots.reponse().Equals("Maintenant que les villes sont placées. Vous pouvez demander la correction"))
                        {
                            sonToutesLesVillesPlacees.Position = new TimeSpan(0);
                            sonToutesLesVillesPlacees.LoadedBehavior = MediaState.Play;
                        }
                        break;
                     case "ZONE6":
                        zoneParis.Opacity = 1;
                        zoneParis.Text = btn1.Text;
                        lesZonesDepots.verifierDoublonsOnMap(zoneParis);
                        zoneParis.TextColor = Brushes.Black;
                        Console.WriteLine("zone6");
                        LogTextBox.Text = "zone6";
                        LogTextBox.Text = lesZonesDepots.reponse();
                        if (lesZonesDepots.reponse().Equals("Maintenant que les villes sont placées. Vous pouvez demander la correction"))
                        {
                            sonToutesLesVillesPlacees.Position = new TimeSpan(0);
                            sonToutesLesVillesPlacees.LoadedBehavior = MediaState.Play;
                        }
                        break;
                     case "ZONE7":
                        zoneLyon.Text = btn1.Text;
                        zoneLyon.Opacity = 1;
                        lesZonesDepots.verifierDoublonsOnMap(zoneLyon);
                        zoneLyon.TextColor = Brushes.Black;
                        Console.WriteLine("zone7");
                        LogTextBox.Text = "zone7";
                        LogTextBox.Text = lesZonesDepots.reponse();
                        if (lesZonesDepots.reponse().Equals("Maintenant que les villes sont placées. Vous pouvez demander la correction"))
                        {
                            sonToutesLesVillesPlacees.Position = new TimeSpan(0);
                            sonToutesLesVillesPlacees.LoadedBehavior = MediaState.Play;
                        }
                        break;
                    case "ZONE8":
                        zoneOrléans.Text = btn1.Text;
                        zoneOrléans.Opacity = 1;
                        lesZonesDepots.verifierDoublonsOnMap(zoneOrléans);
                        zoneOrléans.TextColor = Brushes.Black;
                        Console.WriteLine("zone8");
                        LogTextBox.Text = "zone8";
                        LogTextBox.Text = lesZonesDepots.reponse();
                        if (lesZonesDepots.reponse().Equals("Maintenant que les villes sont placées. Vous pouvez demander la correction"))
                        {
                            sonToutesLesVillesPlacees.Position = new TimeSpan(0);
                            sonToutesLesVillesPlacees.LoadedBehavior = MediaState.Play;
                        }
                        break;
                    case "ZONE9":
                        zoneAix.Text = btn1.Text;
                        zoneAix.Opacity = 1;
                        lesZonesDepots.verifierDoublonsOnMap(zoneAix);
                        zoneAix.TextColor = Brushes.Black;
                        Console.WriteLine("zone9");
                        LogTextBox.Text = "zone9";
                        LogTextBox.Text = lesZonesDepots.reponse();
                        if (lesZonesDepots.reponse().Equals("Maintenant que les villes sont placées. Vous pouvez demander la correction"))
                        {
                            sonToutesLesVillesPlacees.Position = new TimeSpan(0);
                            sonToutesLesVillesPlacees.LoadedBehavior = MediaState.Play;
                        }
                        break;
                    case "ZONE10":
                        zoneToulouse.Text = btn1.Text;
                        zoneToulouse.Opacity = 1;
                        lesZonesDepots.verifierDoublonsOnMap(zoneToulouse);
                        zoneToulouse.TextColor = Brushes.Black;
                        Console.WriteLine("zone10");
                        LogTextBox.Text = "zone10";
                        LogTextBox.Text = lesZonesDepots.reponse();
                        if (lesZonesDepots.reponse().Equals("Maintenant que les villes sont placées. Vous pouvez demander la correction"))
                        {
                            sonToutesLesVillesPlacees.Position = new TimeSpan(0);
                            sonToutesLesVillesPlacees.LoadedBehavior = MediaState.Play;
                        }
                        break;
                    
                    case "NEXT":
                        LogTextBox.Text = "next";
                        suivant.Text = btn1.Text;
                        btn1.Text = precedent.Text;
                        precedent.Text = villes.recupererNomVille(btn1.Text, "next");
                        informationMiageVille();
                        break;
                    case "PREVIOUS":                        
                        precedent.Text = btn1.Text;
                        btn1.Text = suivant.Text;                        
                        suivant.Text = villes.recupererNomVille(btn1.Text, "previous");
                        Console.WriteLine("PREVIOUS");
                        LogTextBox.Text = "previous";
                        informationMiageVille();
                        break;


                    /*case "AIX":
                        villes.getNomVille(2);
                        Console.WriteLine("Aix");
                        btn1.Text = "Aix";
                        break;
                    case "LYON":
                        villes.getNomVille(3);
                        Console.WriteLine("Lyon");
                        btn1.Text = "Lyon";
                        break;
                    case "LILLE":
                        villes.getNomVille(0);
                        Console.WriteLine("Lille");
                        btn1.Text = "Lille";
                        break;
                    case "PARIS":
                        villes.getNomVille(1);
                        Console.WriteLine("Paris");
                        btn1.Text = "Paris";
                        break;
                    case "NANCY":
                        villes.getNomVille(4);
                        Console.WriteLine("Nancy");
                        btn1.Text = "Nancy";
                        break;
                    case "BORDEAUX":
                        villes.getNomVille(5);
                        Console.WriteLine("Bordeaux");
                        btn1.Text = "Bordeaux";
                        break;
                    case "TOULOUSE":
                        villes.getNomVille(6);
                        Console.WriteLine("Toulouse");
                        btn1.Text = "Toulouse";
                        break;
                    case "NANTES":
                        villes.getNomVille(7);
                        Console.WriteLine("Nantes");
                        btn1.Text = "Nantes";
                        break;
                    case "ORLEANS":
                        villes.getNomVille(8);
                        Console.WriteLine("Orléans");
                        btn1.Text = "Orléans";
                        break;
                    case "RENNES":
                        villes.getNomVille(9);
                        Console.WriteLine("Rennes");
                        btn1.Text = "Rennes";
                        break;
                    */
                    case "INITIALISE":
                        Console.WriteLine("initialise");
                        LogTextBox.Text = "initialise";
                        lesZonesDepots.initialise();
                        LogTextBox.Text = "";
                        sonMiseAZero.Position = new TimeSpan(0);
                        sonMiseAZero.LoadedBehavior = MediaState.Play;
                        break;
                    case "CORRECTION":
                        LogTextBox.Text = "correction";
                        Console.WriteLine("Correction");
                        Indication();                     
                        break;
                }
            }
        }
       

        private void Indication()
        {
            lesZonesDepots.verifierCorrespondanceZoneLabel();
            LogTextBox.Text = lesZonesDepots.reponse();
            if (lesZonesDepots.reponse().Equals("CorrectionFaite"))
            {
                sonCorrectionEffectuee.Position = new TimeSpan(0);
                sonCorrectionEffectuee.LoadedBehavior = MediaState.Play;
                LogTextBox.Text = lesZonesDepots.afficheDetailCorrection();
            }            
            else
            {
                sonCorrectionImpossible.Position = new TimeSpan(0);
                sonCorrectionImpossible.LoadedBehavior = MediaState.Play;
            }
        }

        

        

        private void InitialisationVille()
        {
            VillesTextBox.Text = "L'ensemble des villes suivantes sont à placer dans " + "\n" + "les zones de la carte :" +
                "\n" + "\t" + "- Aix" + "\t\t" + " - Nantes"
                + "\n" + "\t" + "- Bordeaux" + "\t" + " - Orléans"
                + "\n" + "\t" + "- Lille" + "\t\t" + " - Paris"
                + "\n" + "\t" + "- Lyon" + "\t\t" + " - Rennes"
                + "\n" + "\t" + "- Nancy" + "\t\t" + " - Toulouse" + "\n"

                + "Pour vérifier l'exactitude des villes veuillez dire : " + "\n"
                + "\t"+"- Correction" + "\n"

                + "Pour annuler les données placées veuillez dire : " + "\n"
                + "\t" + "- Reset" + "\n";        
        }
        private void informationMiageVille()
        {            
            if (btn1.Text == "Lille")
            {
                RennesLogo.Opacity = 0;
                RennesLogo.LoadedBehavior = MediaState.Stop;
                Paris5Logo.Opacity = 0;
                Paris5Logo.LoadedBehavior = MediaState.Stop;

                textBoxInfoVille.Text = InformationVille.informationLille();
                Lille1Logo.Opacity = 1;
                Lille1Logo.LoadedBehavior = MediaState.Play;
            }
            else if (btn1.Text == "Nancy")
            {
                LyonLogo.Opacity = 0;
                LyonLogo.LoadedBehavior = MediaState.Stop;
                BordeauxLogo.Opacity = 0;
                BordeauxLogo.LoadedBehavior = MediaState.Stop;

                textBoxInfoVille.Text = InformationVille.informationNancy();
                NancyLogo.Opacity = 1;
                NancyLogo.LoadedBehavior = MediaState.Play;                
            }
            else if (btn1.Text == "Bordeaux")
            {
                NancyLogo.Opacity = 0;
                NancyLogo.LoadedBehavior = MediaState.Stop;
                ToulouseLogo.Opacity = 0;
                ToulouseLogo.LoadedBehavior = MediaState.Stop;

                textBoxInfoVille.Text = InformationVille.informationBordeaux();
                BordeauxLogo.Opacity = 1;
                BordeauxLogo.LoadedBehavior = MediaState.Play;
            }
            else if (btn1.Text == "Nantes")
            {
                ToulouseLogo.Opacity = 0;
                ToulouseLogo.LoadedBehavior = MediaState.Stop;
                OrleansLogo.Opacity = 0;
                OrleansLogo.LoadedBehavior = MediaState.Stop;

                 textBoxInfoVille.Text = InformationVille.informationNantes();
                 NantesLogo.Opacity = 1;
                 NantesLogo.LoadedBehavior = MediaState.Play;
            }
            else if (btn1.Text == "Rennes")
            {
                Lille1Logo.Opacity = 0;
                Lille1Logo.LoadedBehavior = MediaState.Stop;
                OrleansLogo.Opacity = 0;
                OrleansLogo.LoadedBehavior = MediaState.Stop;

                textBoxInfoVille.Text = InformationVille.informationRennes();
                RennesLogo.Opacity = 1;
                RennesLogo.LoadedBehavior = MediaState.Play;
            }
            else if (btn1.Text == "Paris")
            {
                Lille1Logo.Opacity = 0;
                Lille1Logo.LoadedBehavior = MediaState.Stop;
                aixMarseilleLogo.Opacity = 0;
                aixMarseilleLogo.LoadedBehavior = MediaState.Stop;

                textBoxInfoVille.Text = InformationVille.informationParis();
                Paris5Logo.Opacity = 1;
                Paris5Logo.LoadedBehavior = MediaState.Play;
            }
            else if (btn1.Text == "Lyon")
            {
                aixMarseilleLogo.Opacity = 0;
                aixMarseilleLogo.LoadedBehavior = MediaState.Stop;
                NancyLogo.Opacity = 0;
                NancyLogo.LoadedBehavior = MediaState.Stop;

                textBoxInfoVille.Text = InformationVille.informationLyon();
                LyonLogo.Opacity = 1;
                LyonLogo.LoadedBehavior = MediaState.Play;
            }
            else if (btn1.Text == "Orléans")
            {
                NantesLogo.Opacity = 0;
                NantesLogo.LoadedBehavior = MediaState.Stop;
                RennesLogo.Opacity = 0;
                RennesLogo.LoadedBehavior = MediaState.Stop;

                textBoxInfoVille.Text = InformationVille.informationOrleans();
                OrleansLogo.Opacity = 1;
                OrleansLogo.LoadedBehavior = MediaState.Play;
            }
            else if (btn1.Text == "Aix")
            {
                Paris5Logo.Opacity = 0;
                Paris5Logo.LoadedBehavior = MediaState.Stop;
                LyonLogo.Opacity = 0;
                LyonLogo.LoadedBehavior = MediaState.Stop;

                textBoxInfoVille.Text = InformationVille.informationAix();
                aixMarseilleLogo.Opacity = 1;
                aixMarseilleLogo.LoadedBehavior = MediaState.Play;
            }
            else
            {
                BordeauxLogo.Opacity = 0;
                BordeauxLogo.LoadedBehavior = MediaState.Stop;
                NantesLogo.Opacity = 0;
                NantesLogo.LoadedBehavior = MediaState.Stop;

                textBoxInfoVille.Text = InformationVille.informationToulouse();
                ToulouseLogo.Opacity = 1;
                ToulouseLogo.LoadedBehavior = MediaState.Play;
            }
        }
    }
}


    
        

