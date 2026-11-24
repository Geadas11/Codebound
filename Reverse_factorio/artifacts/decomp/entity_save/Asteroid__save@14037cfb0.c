// Asteroid::save @ 14037cfb0 (function Asteroid::save)

void Asteroid__save(longlong param_1,undefined8 param_2)

{
  EntityWithOwner__save();
  UpdatableEntity__save(param_1 + 0xa8,param_2);
  Vector2<double,Vector>__save(param_1 + 0xd0,param_2);
  BoundingBox__save(param_1 + 0xe0,param_2);
  return;
}

