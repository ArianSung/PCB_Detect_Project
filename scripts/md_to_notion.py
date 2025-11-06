#!/usr/bin/env python3
"""
마크다운을 Notion 블록으로 변환하는 스크립트
"""
import re
import json

def parse_markdown_to_notion_blocks(md_content):
    """마크다운을 Notion 블록 형식으로 변환"""
    lines = md_content.split('\n')
    blocks = []
    in_code_block = False
    code_block_content = []
    code_block_lang = ''
    in_table = False
    table_rows = []

    i = 0
    while i < len(lines):
        line = lines[i]

        # 코드 블록 시작/종료
        if line.startswith('```'):
            if not in_code_block:
                in_code_block = True
                code_block_lang = line[3:].strip() or 'plain text'
                code_block_content = []
            else:
                in_code_block = False
                blocks.append({
                    "object": "block",
                    "type": "code",
                    "code": {
                        "rich_text": [{
                            "type": "text",
                            "text": {
                                "content": '\n'.join(code_block_content)
                            }
                        }],
                        "language": code_block_lang
                    }
                })
                code_block_content = []
                code_block_lang = ''
            i += 1
            continue

        # 코드 블록 내부
        if in_code_block:
            code_block_content.append(line)
            i += 1
            continue

        # 구분선
        if line.strip() == '---':
            blocks.append({
                "object": "block",
                "type": "divider",
                "divider": {}
            })
            i += 1
            continue

        # 제목 (H1)
        if line.startswith('# '):
            text = line[2:].strip()
            blocks.append({
                "object": "block",
                "type": "heading_1",
                "heading_1": {
                    "rich_text": parse_rich_text(text)
                }
            })
            i += 1
            continue

        # 제목 (H2)
        if line.startswith('## '):
            text = line[3:].strip()
            blocks.append({
                "object": "block",
                "type": "heading_2",
                "heading_2": {
                    "rich_text": parse_rich_text(text)
                }
            })
            i += 1
            continue

        # 제목 (H3)
        if line.startswith('### '):
            text = line[4:].strip()
            blocks.append({
                "object": "block",
                "type": "heading_3",
                "heading_3": {
                    "rich_text": parse_rich_text(text)
                }
            })
            i += 1
            continue

        # 제목 (H4)
        if line.startswith('#### '):
            text = line[5:].strip()
            blocks.append({
                "object": "block",
                "type": "heading_3",  # Notion은 H3까지만 지원
                "heading_3": {
                    "rich_text": parse_rich_text(text)
                }
            })
            i += 1
            continue

        # 불릿 리스트
        if line.strip().startswith('- '):
            text = line.strip()[2:]
            blocks.append({
                "object": "block",
                "type": "bulleted_list_item",
                "bulleted_list_item": {
                    "rich_text": parse_rich_text(text)
                }
            })
            i += 1
            continue

        # 번호 리스트
        if re.match(r'^\d+\.\s', line.strip()):
            text = re.sub(r'^\d+\.\s', '', line.strip())
            blocks.append({
                "object": "block",
                "type": "numbered_list_item",
                "numbered_list_item": {
                    "rich_text": parse_rich_text(text)
                }
            })
            i += 1
            continue

        # 테이블 (간단한 처리)
        if '|' in line and line.strip().startswith('|'):
            if not in_table:
                in_table = True
                table_rows = []
            # 테이블 구분선 무시
            if not re.match(r'^\|[\s\-:]+\|', line):
                cells = [cell.strip() for cell in line.split('|')[1:-1]]
                table_rows.append(cells)
            i += 1
            # 다음 줄이 테이블이 아니면 테이블 종료
            if i < len(lines) and ('|' not in lines[i] or not lines[i].strip().startswith('|')):
                # 테이블을 코드 블록으로 변환 (Notion 테이블은 복잡함)
                table_text = '\n'.join(['  '.join(row) for row in table_rows])
                blocks.append({
                    "object": "block",
                    "type": "paragraph",
                    "paragraph": {
                        "rich_text": [{
                            "type": "text",
                            "text": {
                                "content": table_text
                            }
                        }]
                    }
                })
                in_table = False
                table_rows = []
            continue

        # 빈 줄
        if not line.strip():
            i += 1
            continue

        # 일반 텍스트 (paragraph)
        blocks.append({
            "object": "block",
            "type": "paragraph",
            "paragraph": {
                "rich_text": parse_rich_text(line)
            }
        })
        i += 1

    return blocks


def parse_rich_text(text):
    """텍스트에서 bold, italic 등을 파싱하여 rich_text 배열로 변환"""
    rich_text = []

    # 간단한 bold 처리 (**text**)
    parts = re.split(r'(\*\*[^*]+\*\*)', text)

    for part in parts:
        if not part:
            continue

        if part.startswith('**') and part.endswith('**'):
            # Bold
            content = part[2:-2]
            rich_text.append({
                "type": "text",
                "text": {
                    "content": content
                },
                "annotations": {
                    "bold": True
                }
            })
        else:
            # 일반 텍스트
            rich_text.append({
                "type": "text",
                "text": {
                    "content": part
                }
            })

    # rich_text가 비어있으면 빈 텍스트 추가
    if not rich_text:
        rich_text = [{
            "type": "text",
            "text": {
                "content": " "
            }
        }]

    return rich_text


def main():
    # 마크다운 파일 읽기
    with open('/home/sys1041/work_project/docs/프로젝트_설계보고서.md', 'r', encoding='utf-8') as f:
        md_content = f.read()

    # Notion 블록으로 변환
    blocks = parse_markdown_to_notion_blocks(md_content)

    # JSON 파일로 저장 (나중에 Notion API로 전송)
    output = {
        "blocks": blocks,
        "total_blocks": len(blocks),
        "chunks": []
    }

    # 100개씩 나누기
    chunk_size = 100
    for i in range(0, len(blocks), chunk_size):
        chunk = blocks[i:i+chunk_size]
        output["chunks"].append({
            "chunk_index": i // chunk_size,
            "blocks": chunk
        })

    # JSON 파일로 저장
    with open('/home/sys1041/work_project/notion_blocks.json', 'w', encoding='utf-8') as f:
        json.dump(output, f, ensure_ascii=False, indent=2)

    print(f"✓ 총 {len(blocks)}개의 블록이 생성되었습니다.")
    print(f"✓ {len(output['chunks'])}개의 청크로 나누어졌습니다.")
    print(f"✓ JSON 파일이 저장되었습니다: /home/sys1041/work_project/notion_blocks.json")


if __name__ == '__main__':
    main()
