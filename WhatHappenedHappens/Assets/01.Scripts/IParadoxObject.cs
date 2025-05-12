

// [ 상태 스냅샷 저장 및 복원 기능을 제공하는 인터페이스 ]
//
public interface IParadoxObject
{
    ObjectSnapshot CreateSnapshot();
    void RestoreSnapshot(ObjectSnapshot snapshot);
}
