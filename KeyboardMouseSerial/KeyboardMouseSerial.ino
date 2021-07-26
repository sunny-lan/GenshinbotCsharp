

#include "Keyboard.h"
#include "Mouse.h"
enum Command {
  key_down = 'k',
  mouse_move = 'm',
  key_up = 'j',
  mouse_down = 'c',
  mouse_up = 'd',
  toggle_mk = 't',
  release_all = 'r',
  test_mode = 'z',
  wind_mouse = 'w',
  do_nothing = '_'
};
enum MBtn {
  left = 'l',
  middle = 'm',
  right = 'r'
};

struct MouseMoveCommand {
  int16_t x;
  int16_t y;
  int16_t z;
};


bool enabled = false;




struct MOUSEPOINT
{
  int16_t x;
  int16_t y;
};
void moveMouse(MouseMoveCommand d);
void MoveMouse(MOUSEPOINT p)
{
  moveMouse(MouseMoveCommand{(int16_t)p.x, (int16_t)p.y, 0});
}
double rengine() {
  double res= (double)random(1UL << 31) / (double)(1UL << 31);
  return res;
}

MOUSEPOINT WindMouse(double x1, double y1, double x2, double y2, double gravity, double wind, double minWait, double maxWait, double maxStep, double targetArea)
{
  double sqrt3 = sqrt(3.0);
  double sqrt5 =  sqrt(5.0);
  double dist = 0.0, veloX = 0.0, veloY = 0.0, windX = 0.0, windY = 0.0;
  MOUSEPOINT lastPosition = { 0, 0 };


  while ((dist = hypot(x2 - x1, y2 - y1)) >= 1)
  {
    if (Serial.available() > 0){
      return lastPosition;
    }
    wind = min(wind, dist);

    if (dist >= targetArea)
    {
      windX = windX / sqrt3 + (rengine() * (wind * 2.0 + 1.0) - wind) / sqrt5;
      windY = windY / sqrt3 + (rengine() * (wind * 2.0 + 1.0) - wind) / sqrt5;
    }
    else
    {
      windX /= sqrt3;
      windY /= sqrt3;

      if (maxStep < 3)
      {
        maxStep = rengine() * 3 + 3.0;
      }
      else
      {
        maxStep /= sqrt5;
      }
    }

    veloX += windX + gravity * (x2 - x1) / dist;
    veloY += windY + gravity * (y2 - y1) / dist;

    double veloMag = hypot(veloX, veloY);
    if (veloMag > maxStep)
    {
      double randomDist = maxStep / 2.0 + rengine() * maxStep / 2.0;
      veloX = (veloX / veloMag) * randomDist;
      veloY = (veloY / veloMag) * randomDist;
    }

    x1 += veloX;
    y1 += veloY;
    int16_t mx = static_cast<int16_t>(round(x1));
    int16_t my = static_cast<int16_t>(round(y1));

    if (lastPosition.x != mx || lastPosition.y != my)
    {
      MoveMouse(MOUSEPOINT{ mx - lastPosition.x, my - lastPosition.y });
      lastPosition = MOUSEPOINT{ mx, my };
    }

    double step = hypot(x1 - lastPosition.x, y1 - lastPosition.y);
    //cout << "===" << endl;
    //cout << "maxWait: " << maxWait << endl;
    //cout << "minWait: " << minWait << endl;
    //cout << round(maxWait - minWait) << endl;
    //cout << "step: " << step << endl;
    //cout << "maxStep: " << maxStep << endl;
    //cout << (step / maxStep) << endl;


    unsigned long milliseconds = static_cast<unsigned long>(((maxWait - minWait) * (step / maxStep) + minWait));
    //cout << milliseconds << endl;

    delay(milliseconds);
  }

  return lastPosition;
}

MOUSEPOINT WindMouse(MOUSEPOINT p)
{
  MOUSEPOINT lastPosition = { -1, -1 };
  double speed = (rengine() * 15.0 + 15.0) / 10.0;
  return WindMouse(lastPosition.x, lastPosition.y, p.x, p.y, 9.0, 4.0, 5.0 / speed, 10.0 / speed, 10.0 * speed, 8.0 * speed);
}

void setup() {
  Serial.setTimeout((1 << 32L) - 1L);
  // open the serial port:
  Serial.begin(2000000);
  // initialize control over the keyboard:
  Keyboard.begin();
  Mouse.begin();
  randomSeed(analogRead(0));
  enabled = true;
}
template<typename T>
bool readS(T &v) {
  bool res = false;
  if (res = Serial.readBytes((byte*)&v, sizeof(T)) != sizeof(T))
    Serial.println("readbytes failed");
  return res;
}


char cvtBtn( MBtn b) {
  if (b == left) {
    return MOUSE_LEFT;
  } else if (b == right) {
    return MOUSE_RIGHT;
  } else if (b == middle) {
    return MOUSE_MIDDLE;
  } else {
    Serial.println("invalid mouse btn");
    return (char)b;
  }
}

void moveMouse(MouseMoveCommand d) {

  while (d.x != 0 || d.y != 0 || d.z != 0) {
    //Serial.print("bad");
    //Serial.println(d.y);

    int16_t dx = constrain(d.x, (int16_t) - 127, (int16_t)127),
            dy = constrain(d.y, (int16_t) - 127, (int16_t)127),
            dz = constrain(d.z, (int16_t) - 127, (int16_t)127);
    Mouse.move((signed char)dx, (signed char)dy, (signed char)dz);
    //delay(1000);
    d.x -= dx, d.y -= dy, d.z -= dz;
  }
}

void loop() {
  byte tmp; readS(tmp);
  Command cmd = (Command)tmp;
  if (cmd == toggle_mk) {
    if (enabled) {
      Keyboard.releaseAll();
      Mouse.release(MOUSE_LEFT);
      Mouse.release(MOUSE_RIGHT);
      Mouse.release(MOUSE_MIDDLE);
      Keyboard.end();
      Mouse.end();
      enabled = false;
    } else {
      Keyboard.begin();
      Mouse.begin();
      enabled = true;
    }
    Serial.print('a');//ack
    return;
  }
  if (!enabled) {
    Serial.println("command ignored: not enabled");
    return;
  }
  if (cmd == test_mode) {
    char tc; readS(tc);
    if (tc == 'm')
      moveMouse(MouseMoveCommand{0, -500, 0});
    else if (tc == 'w')
      WindMouse(MOUSEPOINT{0, 500});
    else {
      Serial.println("no test");
      return;
    }
  } else if (cmd == do_nothing) {

  }
  else if (cmd == key_down) {
    byte code; readS(code);
    Keyboard.press(code);
  } else if (cmd == key_up) {
    byte code; readS(code);
    Keyboard.release(code);
  } else if (cmd == release_all) {
    Keyboard.releaseAll();
    Mouse.release(MOUSE_LEFT);
    Mouse.release(MOUSE_RIGHT);
    Mouse.release(MOUSE_MIDDLE);
  } else if (cmd == mouse_up) {
    char code; readS(code);
    MBtn btn = (MBtn)code;
    Mouse.release(cvtBtn(code));
  } else if (cmd == mouse_down) {
    char code; readS(code);
    MBtn btn = (MBtn)code;
    Mouse.press(cvtBtn(code));
  } else if (cmd == mouse_move) {
    MouseMoveCommand d; readS(d);

    moveMouse(d);
  } else if (cmd == wind_mouse) {
    MOUSEPOINT d; readS(d);

    WindMouse(d);
  } else {
    Serial.print("invalid cmd");
    return;
  }
  Serial.print('a');//ack
}
