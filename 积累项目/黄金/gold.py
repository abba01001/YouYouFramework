"""
浙商金价实时监控
version: 1.4
只显示黄金价格数字
2025年10月21日
"""
import requests
import tkinter as tk

# 金价 API URL
url = "https://api.jdjygold.com/gw2/generic/jrm/h5/m/stdLatestPrice?productSku=1961543816"

def fetch_data():
    """获取金价并更新界面"""
    try:
        response = requests.get(url, timeout=5)
        response.raise_for_status()
        data = response.json()

        price = data['resultData']['datas']['price']
        price_label.config(text=f"{price}")

    except requests.exceptions.RequestException:
        price_label.config(text="错误")
    except (ValueError, KeyError):
        price_label.config(text="错误")

    # 每秒刷新一次
    root.after(1000, fetch_data)

# 创建主窗口
root = tk.Tk()
#root.configure(bg="#f5f5f5")  # 浅灰背景，接近办公软件
root.title("数据")  # 模糊标题，更隐蔽
root.geometry("200x40")  # 小巧窗口，不占空间
root.resizable(True, True)  # 禁止缩放（避免不小心拉大暴露）
root.attributes('-topmost', True)  # 窗口置顶，方便查看
root.attributes('-alpha', 0.2)  # 透明度 0.7（可改 0.6~0.8 之间）

# 创建标签显示价格
price_label = tk.Label(
    root, 
    text="--", 
    font=("微软雅黑", 14), 
    fg="#333333",  # 稍深的灰色，透明状态下更清晰
    bg="#f5f5f5"   # 和窗口背景一致，减少边缘感
)
price_label.pack(expand=True)



# 启动数据获取
fetch_data()

# 启动 GUI 主循环
root.mainloop()
