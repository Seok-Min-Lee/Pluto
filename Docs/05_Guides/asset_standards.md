# 🎨 에셋 및 리소스 제작 표준 (Asset Standards)

이 문서는 `Pluto` 프로젝트에서 사용되는 아트 및 오디오 리소스의 기술적 규격과 관리 규칙을 정의합니다.

---

## 1. 스프라이트 및 텍스처 (Sprites & Textures)

모든 2D 리소스는 다음 설정을 준수하여 엔진에 임포트합니다.

- **PPU (Pixels Per Unit):** 100 (프로젝트 표준 1:100 준수)
- **Texture Type:** Sprite (2D and UI)
- **Mesh Type:** Tight
- **Compression:** High Quality (모바일 대응 필요 시 별도 압축)

---

## 2. VFX 및 파티클 (Visual Effects)

타격감과 시각적 명확성을 위해 다음 규칙을 따릅니다.

- **Sorting Layer:** 전경(Foreground) 또는 VFX 레이어 사용.
- **Emission:** 과도한 Bloom 효과 방지를 위해 `Intensity` 조절.
- **Object Pooling:** 모든 파티클 프리팹은 `EffectSpawner`를 통해 오브젝트 풀에서 관리.

---

## 3. 오디오 (Audio & SFX)

- **Format:** WAV (원본), MP3 (압축/배포용)
- **Sample Rate:** 44.1kHz
- **Volume:** 리소스 자체의 피크(Peak)를 -3dB 이하로 관리.
- **3D Sound:** 배경음(2D), 효과음(3D Spatial Blend 0.5~1.0).

---

마지막 업데이트: 2026-03-31
