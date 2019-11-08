using URControler2;
using System.Threading;

namespace multimodal
{
    public class RobotMakeDrinkControl
    {
        URServerAction uRServerAction_right, uRServerAction_left;
        public RobotMakeDrinkControl(URServerAction uRServerActionLeft, URServerAction uRServerActionRight)
        {
            uRServerAction_left = uRServerActionLeft;
            uRServerAction_right = uRServerActionRight;
        }
        public void rotate_bottle(float[] bottle_r, float[] cup_r, float[] bottle_l)
        {

            if (uRServerAction_right == null)
            {
                throw new System.IO.DriveNotFoundException("完蛋了。");
            }

            uRServerAction_left.GripperOpen();
            uRServerAction_right.GripperOpen();
            uRServerAction_right.Move(RobotInitial.robot_initial_pos_r);
            uRServerAction_left.Move(RobotInitial.robot_initial_pos_l);
            
            uRServerAction_left.GripperCloseForceMIN();

            uRServerAction_left.Move(new float[] { bottle_l[0] + ImagePosition.image_left_x, 0.01F, bottle_l[1] + ImagePosition.image_left_y - 0.03F, 2.4F, 2.5F, 1.5F });
            uRServerAction_left.Move(new float[] { bottle_l[0] + ImagePosition.image_left_x, 0.04F, bottle_l[1] + ImagePosition.image_left_y - 0.04F, 2.4F, 2.5F, 1.5F });

            uRServerAction_left.ForceMode(0, 30);

            uRServerAction_right.Move(new float[] { bottle_r[0] + ImagePosition.image_right_x, 0.250F, bottle_r[1] + ImagePosition.image_right_y + 0.04F, 2.174F, -2.233F, 0.002F });
            uRServerAction_right.Move(new float[] { bottle_r[0] + ImagePosition.image_right_x, 0.250F, bottle_r[1] + ImagePosition.image_right_y, 2.174F, -2.233F, 0.002F });
            Thread.Sleep(2000);

            uRServerAction_left.EndForceMode();
            uRServerAction_left.Move(new float[] { bottle_l[0] + ImagePosition.image_left_x, 0.01F, bottle_l[1] + ImagePosition.image_left_y - 0.04F, 2.4F, 2.5F, 1.5F });
            uRServerAction_left.Move(RobotInitial.robot_initial_pos_l);
            uRServerAction_left.GripperOpen();


            uRServerAction_right.GripperCloseForceMAX();

            uRServerAction_right.Move(new float[] { bottle_r[0] + ImagePosition.image_right_x, 0.110F, bottle_r[1] + ImagePosition.image_right_y, 2.174F, -2.233F, 0.002F });
            uRServerAction_right.Move(new float[] { RobotInitial.robot_initial_pos_rc[0], 0.110F, bottle_r[1] + ImagePosition.image_right_y, 2.174F, -2.233F, 0.002F });
            uRServerAction_right.Move(RobotInitial.robot_initial_pos_rc);

            Thread.Sleep(2000);
            RobotInitial.robot_initial_pos_lc[1] -= 0.02F;
            uRServerAction_left.Move(RobotInitial.robot_initial_pos_lc);
            RobotInitial.robot_initial_pos_lc[1] += 0.02F;
            uRServerAction_left.Move(RobotInitial.robot_initial_pos_lc);
            Thread.Sleep(1000);


            uRServerAction_left.TurnJoint(4, -15, 2);
            Thread.Sleep(2000);

            uRServerAction_left.Move(RobotInitial.robot_initial_pos_lc);

            uRServerAction_left.GripperCloseForceMIN();

            RobotInitial.robot_initial_pos_lc[1] -= 0.020F;

            uRServerAction_left.Move(RobotInitial.robot_initial_pos_lc);

            Thread.Sleep(2000);

            uRServerAction_right.Move(new float[] { cup_r[0] + 0.015F, 0.110F, cup_r[1] + 0.040F, 2.26F, -2.27F, -0.06F });
            uRServerAction_right.Move(new float[] { cup_r[0] + 0.015F, 0.150F, cup_r[1] + 0.040F, 2.26F, -2.27F, -0.06F });

            uRServerAction_right.MoveJoint(5, 1.75F);
            Thread.Sleep(3000);
            uRServerAction_right.MoveJoint(5, -1.75F);


            uRServerAction_right.Move(RobotInitial.robot_initial_pos_rc);
            Thread.Sleep(2000);

            RobotInitial.robot_initial_pos_lc[1] += 0.020F;
            uRServerAction_left.Move(RobotInitial.robot_initial_pos_lc);
            uRServerAction_left.GripperOpen();

            Thread.Sleep(2000);

            RobotInitial.robot_initial_pos_lc[1] -= 0.01F;
            uRServerAction_left.Move(RobotInitial.robot_initial_pos_lc);
            RobotInitial.robot_initial_pos_lc[1] += 0.01F;

            uRServerAction_left.TurnJoint(-4, 25, 2);

            Thread.Sleep(5000);
            RobotInitial.robot_initial_pos_rc[1] += 0.02F;
            uRServerAction_right.Move(RobotInitial.robot_initial_pos_rc);
            Thread.Sleep(2000);
            RobotInitial.robot_initial_pos_rc[1] -= 0.02F;

            uRServerAction_left.Move(RobotInitial.robot_initial_pos_l);

            uRServerAction_right.Move(new float[] { RobotInitial.robot_initial_pos_rc[0], 0.110F, bottle_r[1] + ImagePosition.image_right_y, 2.174F, -2.233F, 0.002F });
            uRServerAction_right.Move(new float[] { bottle_r[0] + ImagePosition.image_right_x, 0.110F, bottle_r[1] + ImagePosition.image_right_y, 2.26F, -2.27F, -0.06F });

            uRServerAction_right.Move(new float[] { bottle_r[0] + ImagePosition.image_right_x, 0.242F, bottle_r[1] + ImagePosition.image_right_y, 2.26F, -2.27F, -0.06F });
            uRServerAction_right.GripperOpen();
            uRServerAction_right.Move(new float[] { bottle_r[0] + ImagePosition.image_right_x, 0.242F, bottle_r[1] + ImagePosition.image_right_y + 0.03F, 2.26F, -2.27F, -0.06F });

            uRServerAction_right.Move(RobotInitial.robot_initial_pos_r);
            
        }
        public void open_bottle(float[] bottle_r, float[] cup_r, float[] cup_l, float[] bottle_l)
        {
            uRServerAction_left.GripperOpen();
            uRServerAction_right.GripperOpen();
            uRServerAction_right.Move(RobotInitial.robot_initial_pos_r);
            uRServerAction_left.Move(RobotInitial.robot_initial_pos_l);


            uRServerAction_right.Move(new float[] { bottle_r[0] + ImagePosition.image_right_x, -0.015F, bottle_r[1] - 0.080F, 4.724F, 0.001F, -0.034F });
            uRServerAction_right.Move(new float[] { bottle_r[0] + ImagePosition.image_right_x, 0.030F, bottle_r[1] - 0.080F, 4.724F, 0.001F, -0.034F });

            uRServerAction_right.GripperCloseForceMAX();
            Thread.Sleep(1000);

            uRServerAction_right.Move(new float[] {-0.325F, -0.118F, -0.158F, 4.724F, 0.001F, -0.034F });
            uRServerAction_left.Move(new float[] { 0.330F, 0.100F, -0.080F, 0.06F, -3.127F, 0.093F });
            uRServerAction_left.Move(new float[] { 0.330F, 0.100F, -0.120F, 0.06F, -3.127F, 0.093F });
            uRServerAction_left.GripperCloseForceMAX();
            Thread.Sleep(2000);
            uRServerAction_right.GripperOpen();

            uRServerAction_right.Move(new float[] { -0.325F, -0.150F, -0.158F, 4.724F, 0.001F, -0.034F });
            uRServerAction_right.Move(new float[] { -0.325F, -0.150F, 0.100F, 4.724F, 0.001F, -0.034F });
            uRServerAction_right.Move(RobotInitial.robot_initial_pos_r);
            uRServerAction_right.Move(new float[] { -0.3317F, 0.0427F,  0.030F, 2.19F, -2.25F, -0.085F });
            uRServerAction_right.Move(new float[] { -0.3317F, 0.0427F, -0.011F, 2.19F, -2.25F, -0.085F });           
            uRServerAction_right.GripperCloseForceMIN();
            Thread.Sleep(1000);
            uRServerAction_right.GripperOpen();
            uRServerAction_right.Move(new float[] { -0.3317F, 0.0427F, 0.020F, 2.19F, -2.25F, -0.085F });
            uRServerAction_right.Move(RobotInitial.robot_initial_pos_r);

            uRServerAction_left.Move(new float[] { cup_l[0] + ImagePosition.image_left_x - 0.095F, 0.150F, cup_l[1] + ImagePosition.image_left_y - 0.02F, 0.06F, -3.127F, 0.093F });

            uRServerAction_left.MoveJoint(5, -2F);
            Thread.Sleep(5000);
            uRServerAction_left.MoveJoint(5, 2F);

            uRServerAction_right.GripperCloseForceMAX();

            uRServerAction_left.Move(new float[] { 0.330F, 0.100F, -0.120F, 0.06F, -3.127F, 0.093F });
            
            uRServerAction_right.Move(new float[] { -0.2775F, 0.0505F, 0.035F, 2.23F, -2.21F, -0.136F });
            uRServerAction_right.Move(new float[] { -0.2775F, 0.0505F, -0.010F, 2.23F, -2.21F, -0.136F });
            uRServerAction_right.Move(new float[] { -0.268F, -0.035F, -0.010F, 2.23F, -2.21F, -0.136F });
            uRServerAction_right.Move(new float[] { -0.333F, -0.035F, -0.010F, 2.23F, -2.21F, -0.136F });

            uRServerAction_right.ForceMode(0, -28);
            Thread.Sleep(2000);
            uRServerAction_right.EndForceMode();
            uRServerAction_right.Move(new float[] { -0.335F, -0.040F, 0.040F, 2.23F, -2.21F, -0.136F });
            uRServerAction_right.GripperOpen();
            uRServerAction_right.Move(new float[] { -0.325F, -0.150F, -0.158F, 4.724F, 0.001F, -0.034F });
            uRServerAction_right.Move(new float[] { -0.325F, -0.118F, -0.158F, 4.724F, 0.001F, -0.034F });
            uRServerAction_right.GripperCloseForceMIN();
            Thread.Sleep(5000);

            uRServerAction_left.GripperOpen();
            uRServerAction_left.Move(new float[] { 0.330F, 0.100F, -0.080F, 0.06F, -3.127F, 0.093F });         
            uRServerAction_left.Move(RobotInitial.robot_initial_pos_l);
            Thread.Sleep(2000);

            uRServerAction_right.Move(new float[] { bottle_r[0] + ImagePosition.image_right_x, 0.030F, bottle_r[1] - 0.080F, 4.724F, 0.001F, -0.034F });
            uRServerAction_right.GripperOpen();
            uRServerAction_right.Move(new float[] { bottle_r[0] + ImagePosition.image_right_x, -0.015F, bottle_r[1] - 0.080F, 4.724F, 0.001F, -0.034F });
            uRServerAction_right.Move(RobotInitial.robot_initial_pos_r);


        }
        public void pick_bottle(float[] bottle_l, float[] cup_l,bool drink_2)
        {
            uRServerAction_left.GripperOpen();
            uRServerAction_right.GripperOpen();
            uRServerAction_right.Move(RobotInitial.robot_initial_pos_r);
            uRServerAction_left.Move(RobotInitial.robot_initial_pos_l);

            uRServerAction_left.Move(new float[] { bottle_l[0] + ImagePosition.image_left_x, 0.280F, bottle_l[1] + ImagePosition.image_left_y + 0.02F, 0.06F, -3.127F, 0.093F });
            uRServerAction_left.Move(new float[] { bottle_l[0] + ImagePosition.image_left_x , 0.280F, bottle_l[1] + ImagePosition.image_left_y - 0.04F, 0.06F, -3.127F, 0.093F });
            uRServerAction_left.GripperCloseForceMIN();
            Thread.Sleep(1000);
            uRServerAction_left.Move(new float[] { cup_l[0] + ImagePosition.image_left_x - 0.065F, 0.140F, cup_l[1] + ImagePosition.image_left_y - 0.016F, 0.06F, -3.127F, 0.093F });

            uRServerAction_left.MoveJoint(5, -1.5F);
            if (drink_2)
            {
                Thread.Sleep(700);
            }

            uRServerAction_left.MoveJoint(5, 1.5F);

            uRServerAction_left.Move(new float[] { bottle_l[0] + ImagePosition.image_left_x + 0.003F, 0.280F, bottle_l[1] + ImagePosition.image_left_y - 0.04F, 0.06F, -3.127F, 0.093F });
            uRServerAction_left.GripperOpen();
            uRServerAction_left.Move(new float[] { bottle_l[0] + ImagePosition.image_left_x + 0.003F, 0.280F, bottle_l[1] + ImagePosition.image_left_y + 0.01F, 0.06F, -3.127F, 0.093F });

            uRServerAction_left.Move(RobotInitial.robot_initial_pos_l);
        }
        public void pick_cup(float[] cup_r, float[] cup_l)
        {
            uRServerAction_left.GripperCloseForceMIN();
            uRServerAction_right.Move(new float[] { cup_r[0] + ImagePosition.image_right_x + 0.027F, 0.200F, cup_r[1] + 0.045F, 2.1F, -2.1F, -0.38F });
            uRServerAction_right.Move(new float[] { cup_r[0] + ImagePosition.image_right_x + 0.027F, 0.275F, cup_r[1] + 0.045F, 2.1F, -2.1F, -0.38F });
            uRServerAction_right.GripperCloseForceMIN();


            uRServerAction_right.Move(new float[] { cup_r[0] + ImagePosition.image_right_x + 0.027F, 0.050F, cup_r[1] + 0.05F, 2.1F, -2.1F, -0.38F });

            //uRServerAction_left.Move(new float[] { cup_l[0] + ImagePosition.image_left_x - 0.02F, 0.150F, cup_l[1] + ImagePosition.image_left_y, 0.071F, 2.94F, -0.067F});
            //uRServerAction_left.Move(new float[] { cup_l[0] + ImagePosition.image_left_x - 0.02F, 0.135F, cup_l[1] + ImagePosition.image_left_y, 0.071F, 2.94F, -0.067F });
            Thread.Sleep(1000);


            uRServerAction_right.Move(new float[] { -0.500F, 0.050F, -0.085F, 2.1F, -2.1F, -0.38F });
            uRServerAction_left.Move(new float[] { 0.490F, 0.150F, -0.045F, 0.012F, -3.54F, 0.038F });
            uRServerAction_left.Move(new float[] { 0.490F, 0.135F, -0.045F, 0.012F, -3.54F, 0.038F });
            Thread.Sleep(3000);
            uRServerAction_right.GripperOpen();
            uRServerAction_right.Move(new float[] { -0.500F, 0.050F, -0.025F, 2.1F, -2.1F, -0.38F });


            // uRServerAction_left.Move(new float[] { 0.500F, 0.135F, 0.050F, 0.0126F, 2.742F, -0.045F });
            uRServerAction_left.Move(RobotInitial.robot_initial_pos_l);

            //uRServerAction_right.Move(new float[] { cup_r[0] + ImagePosition.image_right_x+0.027F, 0.275F, cup_r[1] + 0.045F, 2.1F, -2.1F, -0.38F });
            //Thread.Sleep(500);
            //uRServerAction_right.GripperOpen();
            //uRServerAction_right.Move(new float[] { cup_r[0] + ImagePosition.image_right_x + 0.027F, 0.2F, cup_r[1] + 0.045F, 2.1F, -2.1F, -0.38F });
            uRServerAction_right.Move(RobotInitial.robot_initial_pos_r);
            uRServerAction_left.GripperOpen();
        }
    }
}
