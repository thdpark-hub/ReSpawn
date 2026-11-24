using System;

// 4개의 재료 ID를 담는 딕셔너리 Key 전용 구조체
public struct RecipeKey : IEquatable<RecipeKey>
{
    public RecipeData id1, id2, id3, id4;

    // 생성자
    public RecipeKey(RecipeData i1, RecipeData i2, RecipeData i3, RecipeData i4)
    {
        id1 = i1; id2 = i2; id3 = i3; id4 = i4;
    }

    // 두 조합식이 완전히 같은지 비교하는 함수 (딕셔너리가 내부적으로 사용함)
    public bool Equals(RecipeKey other)
    {
        return id1 == other.id1 &&
               id2 == other.id2 &&
               id3 == other.id3 &&
               id4 == other.id4;
    }

    // 고유 해시값을 만드는 함수 (C# 8.0 이상에서 지원하는 HashCode.Combine 사용)
    public override int GetHashCode()
    {
        return HashCode.Combine(id1, id2, id3, id4);
    }
}