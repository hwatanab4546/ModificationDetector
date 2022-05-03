
# ModificationDetector

あらかじめ登録しておいたプロパティの値が変更されたことを検出することを目的としたライブラリです。
以前、.NET Framework用にTextBoxなどのコントロールを継承して同等な機能を持つコントロールを作成したことがあるのですが、
そのままではMVVMでは利用しにくいため、MVVMでの利用を想定した形で作成し直しました。

## 特徴

- MVVMのVMのみで操作することを想定しています。
- 任意の時点を初期状態として、変更監視を開始することができます。
- 初期状態を復元することが可能です。

## 使用方法

- 同封のWPFプロジェクト(ModificationDetectorSample)を参考にしてください。

## 備考

- 集成型の取り扱いは苦手です。サンプルプロジェクトでやっているように個々の要素をクラス型にしてゴニョゴニョすれば個々の要素に対する値の変更を検出することは可能ですが、要素の追加や削除といった操作には全く対応できません。
- .NET 6で作成していますが、特別なことをやっているわけではないので.NET Framework 4.X系に書き直すのは容易だと思います。
