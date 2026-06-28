namespace GamePlay
{
    public enum TurnState
    {
        Start,
        PlayerIdle, // 플레이어 입력 대기 상태, 전략 선택 및 블록 배치 가능
        PlayerPlacingContinue, // 플레이어 블록 배치 후 추가 블록 배치 등 후처리
        PlayerPlacingEnd, // 플레이어 블록 변환 애니메이션 재생 중
        EnemyIdle, // 적 턴 대기 상태
        EnemyFlippingTransition, // 적 블록 변환 애니메이션 재생 중,
        End
    }
}