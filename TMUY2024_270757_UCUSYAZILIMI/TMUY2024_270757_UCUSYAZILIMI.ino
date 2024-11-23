/*
 *          EEPROM okumaları yok
 *          EEPROM'a kaydedilecek veriler: |imu ve bmp set bayrakları|, |ayrılma bayrakları|, |saha bayrak|, |statu ve RHRH|, |Paket no|
 *                                                                     ayrıl,otonom,MUYayrilkontrol
 */


#include "I2Cdev.h"
#include "MPU6050_6Axis_MotionApps20.h"
extern TwoWire Wire1;
#include <DFRobot_BMP3XX.h>

#include <SoftwareSerial.h>
#include <TinyGPSPlus.h>
#include <EEPROM.h>
#include "RTClib.h"
#include <Servo.h>
#include <Wire.h>
#include <SPI.h>
#include <SD.h>

#if I2CDEV_IMPLEMENTATION == I2CDEV_ARDUINO_WIRE
    #include "Wire.h"
#endif

#define OUTPUT_READABLE_EULER
#define OUTPUT_READABLE_YAWPITCHROLL
#define limitSwitch_pin 41
#define buzzer_Pin 40
#define mosfet_Pin 37
#define servo_Pin 33       
#define pil_pin 39
#define paketHIGH 1
#define paketLOW 0

#define yukseklikSetBayrakHIGH 3
#define yukseklikSetBayrakLOW 2
#define sahaYukseklikSetBayrakHIGH 5
#define sahaYukseklikSetBayrakLOW 4
#define statuHIGH 7
#define statuLOW 6
#define imuBayrakHIGH 7
#define imuBayrakLOW 6




DFRobot_BMP388_I2C bmp388(&Wire2, bmp388.eSDOGND);
TinyGPSPlus gps;
MPU6050 mpu(0x68,&Wire1);

SoftwareSerial IoTSerial(21,20);
SoftwareSerial yerSerial(15,14);
SoftwareSerial sahaSerial(7, 8);
SoftwareSerial gpsSerial(0, 1);


// MPU control/status vars
bool dmpReady = false;  // set true if DMP init was successful
uint8_t mpuIntStatus;   // holds actual interrupt status byte from MPU
uint8_t devStatus;      // return status after each device operation (0 = success, !0 = error)
uint16_t packetSize;    // expected DMP packet size (default is 42 bytes)
uint16_t fifoCount;     // count of all bytes currently in FIFO
uint8_t fifoBuffer[64]; // FIFO storage buffer

// orientation/motion vars
Quaternion q;           // [w, x, y, z]         quaternion container
VectorInt16 aa;         // [x, y, z]            accel sensor measurements
VectorInt16 aaReal;     // [x, y, z]            gravity-free accel sensor measurements
VectorInt16 aaWorld;    // [x, y, z]            world-frame accel sensor measurements
VectorFloat gravity;    // [x, y, z]            gravity vector
float euler[3];         // [psi, theta, phi]    Euler angle container
float ypr[3];           // [yaw, pitch, roll]   yaw/pitch/roll container and gravity vector

// packet structure for InvenSense teapot demo
uint8_t teapotPacket[14] = { '$', 0x02, 0,0, 0,0, 0,0, 0,0, 0x00, 0x00, '\r', '\n' };
volatile bool mpuInterrupt = false;     // indicates whether MPU interrupt pin has gone high
void dmpDataReady() {
    mpuInterrupt = true;
}


unsigned long long int presentTime, mosfetMillis;      // Otonom ayrılma için millis gelecek aşağıya yazıldı
unsigned long long int prevTime = 0;
String telemetri, rhrh, reelRHRH, aras = "00000", iot = "0.00", gelArtik;
float basinc1, basinc2, yukseklik1, yukseklik2, irtifa, hiz, sicaklik,
          pil=0.0, latiGPS, longiGPS, altiGPS, pitch, roll, yaw, yukseklikEski = 0.0, inis_kontrol, sahaSeaLevel;
int paketNo = 1, statu = 0, mekanikSayac = 0, kurtarma=0;
bool mekanik = false,son = true, kontrol = false, ayril_sw = false, MUY_Ayrilma_Kontrol = false, kurtar=false, ayrilBayrak = false, sahaBayrak = true, imuBayrak = true, yukseklikSetBayrak = true, otonomAyrilBayrak = false, otonomGiris = true;    // Bayrakların gereksiz olanları kaldıracağız
File sdFile;
Servo servo_disk;
RTC_DS1307 rtc;
DateTime now;
char a = 'SOH';

String header = "PAKET NUMARASI;UYDU STATUSU;HATA KODU;GONDERME SAATI;BASINC1;BASINC2;YUKSEKLIK1;YUKSEKLIK2;IRTIFA FARKI;INIS HIZI;SICAKLIK;PIL GERILIMI;GPS1 LATITUDE;GPS1 LONGITUDE;GPS1 ALTITUDE;PITCH;ROLL;YAW;RHRH;IOT;TAKIM NO";


void setup() 
{
  Serial.begin(9600);
  gpsSerial.begin(9600);
  sahaSerial.begin(9600);
  yerSerial.begin(19200);
  IoTSerial.begin(115200);
 
  
  if(imuBayrak)
  {
    #if I2CDEV_IMPLEMENTATION == I2CDEV_ARDUINO_WIRE
          Wire1.begin();
          Wire1.setClock(400000); // 400kHz I2C clock. Comment this line if having compilation difficulties
      #elif I2CDEV_IMPLEMENTATION == I2CDEV_BUILTIN_FASTWIRE
          Fastwire::setup(400, true);
      #endif
    mpu.initialize();
    Serial.println(F("Testing device connections..."));
    Serial.println(mpu.testConnection() ? F("MPU6050 connection successful") : F("MPU6050 connection failed"));
    Serial.println(F("Initializing DMP..."));
    devStatus = mpu.dmpInitialize();
     // supply your own gyro offsets here, scaled for min sensitivity
    mpu.setXGyroOffset(220);
    mpu.setYGyroOffset(76);
    mpu.setZGyroOffset(-85);

    mpu.setZAccelOffset(1788); // 1688 factory default for my test chip
    // make sure it worked (returns 0 if so)
    if (devStatus == 0) 
    {
      // Calibration Time: generate offsets and calibrate our MPU6050
      mpu.CalibrateAccel(6);
      mpu.CalibrateGyro(6);
      mpu.PrintActiveOffsets();
      // turn on the DMP, now that it's ready
      Serial.println(F("Enabling DMP..."));
      mpu.setDMPEnabled(true);
      mpuIntStatus = mpu.getIntStatus();
      dmpReady = true;
      packetSize = mpu.dmpGetFIFOPacketSize();
    }
    else {
        // ERROR!
        // 1 = initial memory load failed
        // 2 = DMP configuration updates failed
        // (if it's going to break, usually the code will be 1)
        Serial.print(F("DMP Initialization failed (code "));
        Serial.print(devStatus);
        Serial.println(F(")"));
      }
    imuBayrak = false;
  }

  while (sahaSerial.available() && sahaBayrak)
  {
    String ali = sahaSerial.readStringUntil('*');
    sahaSeaLevel = ali.toFloat();
    Serial.println(sahaSeaLevel);
    Serial.println(sahaSeaLevel);
    sahaBayrak = false;
  }

 int rslt;                                           //BMP388 çalışma kontrolü
  while( ERR_OK != (rslt = bmp388.begin()) )
  {
    if(ERR_DATA_BUS == rslt){
      Serial.println("Data bus error!!!");
    }else if(ERR_IC_VERSION == rslt){
      Serial.println("Chip versions do not match!!!");
    }
    delay(500);
  }


  /*
   * 6 commonly used sampling modes that allows users to configure easily, mode:
   *      eUltraLowPrecision, Ultra-low precision, suitable for monitoring weather (lowest power consumption), the power is mandatory mode.
   *      eLowPrecision, Low precision, suitable for random detection, power is normal mode
   *      eNormalPrecision1, Normal precision 1, suitable for dynamic detection on handheld devices (e.g on mobile phones), power is normal mode.
   *      eNormalPrecision2, Normal precision 2, suitable for drones, power is normal mode.
   *      eHighPrecision, High precision, suitable for low-power handled devices (e.g mobile phones), power is in normal mode.
   *      eUltraPrecision, Ultra-high precision, suitable for indoor navigation, its acquisition rate will be extremely low, and the acquisition cycle is 1000 ms.
   */
  while(!bmp388.setSamplingMode(bmp388.eUltraPrecision))
  {
    Serial.println("Set samping mode fail, retrying....");
    delay(500);
  }

  // Kalibrasyon DfRobot BMP388 üzerinde fonk ile yapılıyor garip
  if(bmp388.calibratedAbsoluteDifference(0.09) && yukseklikSetBayrak)
  {
    Serial.println("Yukseklik 0 setlendi");
    yukseklikSetBayrak = false;
  }
  

  if (!rtc.begin())              //RTC çalışma kontrol
  {
    Serial.println("!!! RTC hata verdi !!!");
    Serial.flush();
    //while (1);
  }

  servo_disk.attach(servo_Pin);                      
  servo_disk.write(1);  // Başlangıç için N konumu
  delay(400);

  pinMode(mosfet_Pin, OUTPUT);                
  digitalWrite(mosfet_Pin,LOW); 
  pinMode(limitSwitch_pin, INPUT_PULLUP);
  pinMode(pil_pin,INPUT);
  pinMode(buzzer_Pin,OUTPUT);

  SD.begin(BUILTIN_SDCARD);                   
  sdFile = SD.open("TMUY2024_270757_TLM.csv", FILE_WRITE);
      if(!sdFile)
      {
        Serial.println("!!! SD Kart açılmadı !!!");
        //while(1);                                    //SD kart takıldıktan sonra burayı aç
      }
      else
      {
        sdFile.println(header);
        sdFile.close();
      }

  //paketNo = eeprom_read(paketLOW,paketHIGH);
   /* statu = eeprom_read(statuLOW,statuHIGH);


    diğerleri eklenecek     --->                          BAYRAKLAR
    paketNo = eeprom_read(paketLOW,paketHIGH);
    paketNo = eeprom_read(paketLOW,paketHIGH);
  */
}






void loop() 
{
  now = rtc.now();
  BMP_Data();
  SAHA();
  Komut();
  IMU();
  GPS();
  


  if(ayril_sw && digitalRead(limitSwitch_pin))   // Ayrılmanın flag sonrası okunması
  {
    MUY_Ayrilma_Kontrol = true;
  }

  if(!(digitalRead(limitSwitch_pin)))  // Ayrılma sw için ilk bayrak
  {
    ayril_sw = true;
  }

  

  if(ayrilBayrak)   // Komut ile gelen ayrılmanın kapatma işlemi
  {
    unsigned long currentMillis = millis();
    if((currentMillis - mosfetMillis) > 999)
    {
      digitalWrite(mosfet_Pin,LOW);
      ayrilBayrak = false;
    }
  }




  
  if(otonomAyrilBayrak)
  {
    unsigned long currentMillisMosfet = millis();
    if((currentMillisMosfet - mosfetMillis) > 999)
    {
      digitalWrite(mosfet_Pin,LOW);
      otonomAyrilBayrak = false;
      Serial.println("OTONOM BİTTİ");
    }
  }
  
  if((statu > 2 && yukseklik1 < 419.0) && otonomGiris)          // Deneme için şuanlık böyle dursun test edelim burada millis için 2 farklı 
  {                                                             //  şekilde yazmak lazım komut ve otonomu ayırmak için  
    otonomAyrilBayrak = true;
    mosfetMillis = millis();
    digitalWrite(mosfet_Pin,HIGH);
    Serial.println("OTONOM BAŞLADI");
    otonomGiris = false;
  }
  



  if (IoTSerial.available() > 0)
  {
    iot = IoTSerial.readStringUntil('&');
  }


    presentTime = millis();
    if (presentTime - prevTime > 1000)
    {
      float adcVal=analogRead(pil_pin);
      float voltage = adcVal * (3.33 / 1024.0);
      pil = (12.6 * (voltage ) / 3.3) ;

      if(kurtar)         
        kurtarma+=1;

      Mekanik_Filtre();
      Uydu_Statuleri();
      ARAS();
      Telemetri_Paketi();

      prevTime = presentTime;
    }
}




                                           

void Telemetri_Paketi()
{
  telemetri += String(paketNo) + ";";
  telemetri += String(statu) + ";";
  telemetri += String(aras) + ";";
  telemetri += String(now.day()) + "/";
  telemetri += String(now.month()) + "/";
  telemetri += String(now.year()) + ",";
  telemetri += String(now.hour()) + "/";
  telemetri += String(now.minute()) + "/";
  telemetri += String(now.second()) + ";";
  telemetri += String(basinc1) + ";";
  telemetri += String(basinc2) + ";";
  telemetri += String(yukseklik1) + ";";
  telemetri += String(yukseklik2) + ";";
  telemetri += String(irtifa) + ";";
  telemetri += String(hiz) + ";";
  telemetri += String(sicaklik) + ";";
  telemetri += String(pil) + ";";
  telemetri += String(latiGPS, 6) + ";";
  telemetri += String(longiGPS, 6) + ";";
  telemetri += String(altiGPS) + ";";
  telemetri += String(pitch) + ";";
  telemetri += String(roll) + ";";
  telemetri += String(yaw) + ";";
  telemetri += String(reelRHRH) + ";";
  telemetri += String(iot) + ";";
  telemetri += "270757%";


  if (kurtarma<11)
  {
    

    yerSerial.write((byte)0x00); //Alıcı Adresi HIGH
    yerSerial.write(0x02);       //Alıcı Adresi LOW
    yerSerial.write(0x09);       //Alıcı Kanalı =0x17=23    (410M+23=433 MHz)

    Serial.println(telemetri);
    yerSerial.print(telemetri);

    sdFile = SD.open("TMUY2024_270757_TLM.csv", FILE_WRITE);
    if(sdFile)
    {
      sdFile.println(telemetri);
      sdFile.close();
    }
    else{
      Serial.println(" !!! SD Karta Yazılamadı !!! ");
    }
  }

  paketNo++;
  //eeprom_write(paketLOW, paketHIGH, paketNo);
  //eeprom_write(statuLOW, statuHIGH, statu);
  //eeprom_write(sahaYukseklikSetBayrakLOW, sahaYukseklikSetBayrakHIGH, sahaBayrak);
  //eeprom_write(yukseklikSetBayrakLOW, yukseklikSetBayrakHIGH, paketNo);        ---Bayrak ayarlanacak setleme için tek seferlik imu için de yazılacak
  //eeprom_write(imuBayrakLOW, imuBayrakHIGH,imuBayrak);

  yukseklikEski = yukseklik1;
  telemetri = "";
  iot = "0.0";
  basinc2 = 0.0;
  yukseklik2 = 0.0;
  irtifa = 0.0;
}

void BMP_Data()                                  
{                                                 
  basinc1 = bmp388.readPressPa() / 100.0;
  yukseklik1 = bmp388.readAltitudeM();
  sicaklik = bmp388.readTempC();
}

void IMU()
{
  if (!dmpReady) return;
    // read a packet from FIFO
    if (mpu.dmpGetCurrentFIFOPacket(fifoBuffer)) 
    { 
      // Get the Latest packet 
        /*#ifdef OUTPUT_READABLE_QUATERNION
            // display quaternion values in easy matrix form: w x y z
            mpu.dmpGetQuaternion(&q, fifoBuffer);
            Serial.print("quat\t");
            Serial.print(q.w);
            Serial.print("\t");
            Serial.print(q.x);
            Serial.print("\t");
            Serial.print(q.y);
            Serial.print("\t");
            Serial.println(q.z);
        #endif*/

        #ifdef OUTPUT_READABLE_EULER
            // display Euler angles in degrees
            mpu.dmpGetQuaternion(&q, fifoBuffer);
            mpu.dmpGetEuler(euler, &q);
            yaw = euler[0] * 180/M_PI;
            pitch = euler[1] * 180/M_PI;
            roll = euler[2] * 180/M_PI;
          
        #endif

        /*#ifdef OUTPUT_READABLE_YAWPITCHROLL
            // display Euler angles in degrees
            mpu.dmpGetQuaternion(&q, fifoBuffer);
            mpu.dmpGetGravity(&gravity, &q);
            mpu.dmpGetYawPitchRoll(ypr, &q, &gravity);
            Serial.print("ypr\t");
            Serial.print(ypr[0] * 180/M_PI);
            Serial.print("\t");
            Serial.print(ypr[1] * 180/M_PI);
            Serial.print("\t");
            Serial.println(ypr[2] * 180/M_PI);
        #endif*/
    }
}

void Komut()
{
  if(yerSerial.available())
  {
    gelArtik = yerSerial.readStringUntil(a);
    
      if(gelArtik == "AYRIL")
      {
        ayrilBayrak = true;
        mosfetMillis = millis();
        digitalWrite(mosfet_Pin,HIGH);
        Serial.println("AYRILDI");
      }
      
      if(gelArtik.substring(0,4) == "DISK")
      {
        rhrh = gelArtik.substring(4,8);
        reelRHRH = gelArtik.substring(4,8);
        mekanik = true;
        son = true;
      }
  }
}



void SAHA()
{
  if (sahaSerial.available() && aras[4] == '0') // ayrılma koşulu atılacak
  {
    //basinc2 = sahaSerial.readStringUntil('*').toFloat(); // denensin 2 satırdan tek satıra düşübilir.
    // Yukarıdakine göre aşağıdaki daha güvenli gibi
    String rc = sahaSerial.readStringUntil('*');
    basinc2 = rc.toFloat();
    //basinc2 = sahaSerial.readStringUntil('*').toFloat();
    yukseklik2 = bmp388.acilTasiyici(basinc2 , sahaSeaLevel);
    irtifa = abs(yukseklik1 - yukseklik2);
    if(yukseklik2 > 1500.0)
    {
      basinc2 = 0.0;
      yukseklik2 = 0.0;
      irtifa = 0.0;
    }
    Serial.println("SAHA");
  }


}


void Mekanik_Filtre()
{
  if(mekanik && son)
  {

    if((rhrh[1] == 'R' || rhrh[1] == 'r'))
    {
      servo_disk.write(60);     // R -> 60 derece
      rhrh[1] = "";
    }

    if((rhrh[1] == 'G' || rhrh[1] == 'g'))
    {
      servo_disk.write(122);    // G -> 125 derece
      rhrh[1] = "";
    }

    if((rhrh[1] == 'B' || rhrh[1] == 'b'))
    {
      servo_disk.write(180);    // B -> 180 derece
      rhrh[1] = "";
    }
    else
    {
      Serial.println(rhrh);
      Serial.println(mekanikSayac);
      Serial.println(rhrh[0]);
      mekanikSayac++;                      //Kaç saniye filtre olacağını sayan sayac (1Hz)

      if(mekanikSayac == rhrh[0] - '0') /* İlk değer için saniye sayacı eğer süreyi tamamlarsa
                                                 2. Dönme için yapılacak işlemlere girecek */
      {
        Serial.println("sayac önü");
        mekanikSayac = -1; /* İlk filtreden sonra 2. Dönüş için
                                    Hem dönerken hemde sonra N ye direk dönüyor o yüzden
                                    Hem gelirken hem de giderken 1 sn kayıp oluyor onun telafisi */
        if(kontrol)              
        {    
                                 // 2. Dönüş bittikden sonra N ye dönüş
          servo_disk.write(1);
          son = false;              
          kontrol = false;          // Tekrar çalışmaması için oluşturulan bayrak
          mekanikSayac = 0;
          Serial.println("bitti");
        }
        else {
        Serial.println("değişti");
        rhrh[1] = rhrh[3];          // 2. Dönüş için 2. Dönüş parametrelerini 1. Dönüş parametrelerine verdik
        rhrh[0] = rhrh[2];
        kontrol=true;               // 2. dönüşden sonra bitiş için bayrak
        }
      }
    }
  }
}



void GPS()
{
  do
  {
    latiGPS = gps.location.lat();
    longiGPS = gps.location.lng();

    altiGPS = gps.altitude.meters();

    gps.encode(gpsSerial.read());
  }while(gpsSerial.available());
}



void ARAS()
{        
  if( (statu == 2) && !(hiz < 12.00 || hiz > 14.00)) // Ayrılmaya geçerken hız doğru olsa bile düzgün göstermeyecek
  {
    aras[0] = '0';
  }
  else{
    aras[0] = '1';
  }

  if( (statu >= 4) && !(hiz < 6.00 || hiz > 8.00))
  {
    aras[1] = '0';
  }
  else {
    aras[1] = '1';
  }

  if(basinc2 == 0.0)
  {
    aras[2] = '1';
  }
  else {
    aras[2] = '0';
  }

  if(latiGPS == 0.00 || longiGPS == 0.00)
  {
    aras[3] = '1';
  }
  else {
    aras[3] = '0';
  }

  if(irtifa > 5 || MUY_Ayrilma_Kontrol) // Test için or yarışma için and kapsısı  
  {
    aras[4] = '0';
  }
  else {
    aras[4] = '1';
  }
}

void Uydu_Statuleri()    // uydu statüsü eeprom a kaydedilecek
{
  inis_kontrol = yukseklik1 - yukseklikEski;
  hiz = abs(inis_kontrol);

  if(statu == 0 && yukseklik1 > 7.0) 
  {
    statu = 1;
  }

  if(statu == 1 && inis_kontrol < 0.0)   // 700 metrenin altının kontrolü negatiflik
  {
   statu = 2;
  }

   if(statu == 2 && yukseklik1 < 410.0)    //410 aşağısı ayrılma bölgesi
   {
    statu = 3;
   }

   if(statu == 3 && yukseklik1 < 390.0)   // 390 ayrılmanın alt bölgesi
   {
    statu = 4;
   }

   if(statu == 4 &&  yukseklik1 < 7.0)
   {
    statu = 5;
    //kurtarmaSayac = true;
    digitalWrite(buzzer_Pin,HIGH);
   }
}

void eeprom_write(int low, int high, int values)
{
  byte lowbyte = lowByte(values);
  byte highbyte = highByte(values);
  EEPROM.update(low, lowbyte);
  EEPROM.update(high, highbyte);
}

int eeprom_read(int read_low, int read_high)
{
  byte  low = EEPROM.read(read_low);
  byte high = EEPROM.read(read_high);
  int recovery = low + (high << 8);
  return recovery;
}
